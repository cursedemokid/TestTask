using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using AngleSharp.Html.Parser;
using Dapper;
using Npgsql;
using TestTask.Models;

namespace RbcTestTask.Api.Services;

public class ProcessingService
{
    private readonly IConfiguration _configuration;
    // Скомпилированное регулярное выражение для email
    private static readonly Regex EmailRegex = new(
        @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public ProcessingService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<ResponseModel> ProcessAsync(RequestModel request)
    {
        var response = new ResponseModel();

        try
        {
            // 1. Декодирование Base64
            string decodedUrl = Encoding.UTF8.GetString(Convert.FromBase64String(request.UrlB64));
            string decodedHtml = Encoding.UTF8.GetString(Convert.FromBase64String(request.PageB64));

            response.Url = decodedUrl;
            // Validate/decrypt before any database writes.
            response.DecryptedPlainText = DecryptAesEcb(request.EncryptedTextBytesB64, request.KeyBytesB64);

            // 2. Парсинг HTML с помощью AngleSharp
            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(decodedHtml);

            var elements = document.QuerySelectorAll(request.Selector).ToList(); 
            response.ElementsCount = elements.Count;

            var attrList = new List<string>();
            var entitiesToSave = new List<ElementEntity>();

            foreach (var element in elements)
            {
                var attrValue = element.GetAttribute(request.Attribute) ?? string.Empty;
                attrList.Add(attrValue);
                entitiesToSave.Add(new ElementEntity
                {
                    AttributeValue = attrValue,
                    FullHtml = element.OuterHtml
                });
            }
            response.ElementsAttrList = attrList;

            // 3. Сохранение в БД с помощью Dapper
            if (entitiesToSave.Any())
            {
                using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                // Убедимся, что таблица существует (для простоты теста)
                await connection.ExecuteAsync(@"
                    CREATE TABLE IF NOT EXISTS elements (
                        id BIGSERIAL PRIMARY KEY,
                        attribute_value TEXT,
                        full_html TEXT
                    );");

                // Вставка данных
                var insertQuery = @"INSERT INTO elements (attribute_value, full_html) VALUES (@AttributeValue, @FullHtml);";
                await connection.ExecuteAsync(insertQuery, entitiesToSave);
            }

            // 4. Поиск email с помощью регулярного выражения
            var matches = EmailRegex.Matches(decodedHtml);
            var emails = matches.Select(m => m.Value).Distinct().ToList(); // Distinct для уникальности
            response.EmailsCount = emails.Count;
            response.EmailsList = emails;

            // 5. Расшифровка AES-256 ECB
            response.IsError = 0;
        }
        catch (CryptographicException)
        {
            response.IsError = 1;
            response.ErrorCode = "DecryptionError";
            response.ErrorMessage = "Invalid AES-256 key or ciphertext.";
        }
        catch (FormatException ex)
        {
            response.IsError = 1;
            response.ErrorCode = "Base64DecodeError";
            response.ErrorMessage = $"Base64 decoding error: {ex.Message}";
        }
        catch (Exception ex)
        {
            response.IsError = 1;
            response.ErrorCode = "InternalError";
            response.ErrorMessage = ex.Message;
        }

        return response;
    }

    private static string DecryptAesEcb(string encryptedTextB64, string keyB64)
    {
        byte[] cipherText = Convert.FromBase64String(encryptedTextB64);
        byte[] key = Convert.FromBase64String(keyB64);
        if (key.Length != 32 || cipherText.Length == 0 || cipherText.Length % 16 != 0)
            throw new CryptographicException("Invalid AES-256 input.");

        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.None;
        using var decryptor = aes.CreateDecryptor();
        byte[] plainBytes = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
        return Encoding.UTF8.GetString(plainBytes).TrimEnd('\0');
    }
}
