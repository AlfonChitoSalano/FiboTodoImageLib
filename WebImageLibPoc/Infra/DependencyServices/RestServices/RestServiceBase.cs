using Newtonsoft.Json;
using Polly;
using System.Diagnostics;
using System.Text;

namespace WebImageLibPoc.Infra
{
    public abstract class RestServiceBase
    {
        private const string BaseApi = "https://az-func-storage-poc.azurewebsites.net/api/";

        protected readonly string BaseFunctionApi;

        protected RestServiceBase(IConfiguration configuration)
        {
            BaseFunctionApi = BaseApi;
        }

        protected static StringContent CastToStringContent<T>(object? item)
        {
            if (item == null)
            {
                return new StringContent("");
            }

            var jsonString = JsonConvert.SerializeObject((T)item);
            var jsonContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            return jsonContent;
        }

        protected static async Task<T?> GetRemoteAsync<T>(string endPoint)
        {
            string errorResult;

            try
            {
                var jsonResponse = await Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync
                    (
                        retryCount: 3,
                        sleepDurationProvider: _ => TimeSpan.FromSeconds(3)
                    )
                    .ExecuteAsync(async () =>
                    {
                        using var httpClient = await GetClientWithTokenAsync();
                        var result = await httpClient.GetAsync(endPoint).ConfigureAwait(false);
                        return result;
                    });

                if (jsonResponse.IsSuccessStatusCode)
                {
                    var jsonResult = await jsonResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (string.IsNullOrWhiteSpace(jsonResult))
                    {
                        return default;
                    }

                    var deserializedObject = JsonConvert.DeserializeObject<T>(jsonResult);
                    return deserializedObject;
                }

                errorResult = await jsonResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(
                    $"An error was in RestServiceBase.GetRemoteAsync with message {exception.Message}");

                if (exception.InnerException != null)
                    Debug.WriteLine(
                        $"An error was in RestServiceBase.GetRemoteAsync with message {exception.InnerException.Message}");

                throw;
            }

            throw new ArgumentException(
                $"Not successful in getting results from this api {endPoint}. The response error was {errorResult}.");
        }

        protected static async Task<T> PostRemoteAsync<T>(string endPoint, HttpContent httpContent)
        {
            string errorResult;

            try
            {
                var jsonResponse = await Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync
                    (
                        retryCount: 3,
                        sleepDurationProvider: _ => TimeSpan.FromSeconds(3)
                    )
                    .ExecuteAsync(async () =>
                    {
                        using var httpClient = await GetClientWithTokenAsync();
                        var result = await httpClient.PostAsync(endPoint, httpContent).ConfigureAwait(false);
                        return result;
                    });

                if (jsonResponse.IsSuccessStatusCode)
                {
                    var jsonResult = await jsonResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (typeof(T) == typeof(string))
                        return (T)Convert.ChangeType(jsonResult, typeof(string));

                    if (!string.IsNullOrWhiteSpace(jsonResult))
                    {
                        var deserializedObject = JsonConvert.DeserializeObject<T>(jsonResult);
                        return deserializedObject;
                    }

                    errorResult = "JsonResult is null or empty for PostRemoteAsync";
                }
                else
                    errorResult = await jsonResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(
                    $"An error was in RestServiceBase.GetRemoteAsync with message {exception.Message}");

                if (exception.InnerException != null)
                    Debug.WriteLine(
                        $"An error was in RestServiceBase.GetRemoteAsync with message {exception.InnerException.Message}");

                throw;
            }

            throw new ArgumentException(
                $"Not successful in getting results from this api {endPoint}. The response error was {errorResult}.");
        }

        protected async Task PostRemoteAsync(string endPoint, HttpContent httpContent)
        {
            try
            {
                var jsonResponse = await Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync
                    (
                        retryCount: 3,
                        sleepDurationProvider: _ => TimeSpan.FromSeconds(3)
                    )
                    .ExecuteAsync(async () =>
                    {
                        using var httpClient = await GetClientWithTokenAsync();
                        return await httpClient.PostAsync(endPoint, httpContent).ConfigureAwait(false);
                    });

                if (!jsonResponse.IsSuccessStatusCode)
                {
                    var errorResult = await jsonResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    throw new ArgumentException(
                        $"Not successful in getting results from this api {endPoint}. The response error was {errorResult}.");
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine(
                    $"An error was in RestServiceBase.GetRemoteAsync with message {exception.Message}");

                if (exception.InnerException != null)
                    Debug.WriteLine(
                        $"An error was in RestServiceBase.GetRemoteAsync with message {exception.InnerException.Message}");

                throw;
            }
        }

        protected static async Task<T> DeleteRemoteAsync<T>(string endPoint)
        {
            string errorResult;

            try
            {
                var jsonResponse = await Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync
                    (
                        retryCount: 3,
                        sleepDurationProvider: _ => TimeSpan.FromSeconds(3)
                    )
                    .ExecuteAsync(async () =>
                    {
                        using var httpClient = await GetClientWithTokenAsync();
                        var result = await httpClient.DeleteAsync(endPoint).ConfigureAwait(false);
                        return result;
                    });

                if (jsonResponse.IsSuccessStatusCode)
                {
                    var jsonResult = await jsonResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (!string.IsNullOrWhiteSpace(jsonResult))
                    {
                        var deserializedObject = JsonConvert.DeserializeObject<T>(jsonResult);
                        return deserializedObject;
                    }

                    errorResult = "JsonResult is null or empty for DeleteRemoteAsync";
                }
                else
                    errorResult = await jsonResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(
                    $"An error was in GatewayServiceBase.DeleteRemoteAsync with message {exception.Message}");

                if (exception.InnerException != null)
                    Debug.WriteLine(
                        $"An error was in GatewayServiceBase.DeleteRemoteAsync with message {exception.InnerException.Message}");

                throw;
            }

            throw new ArgumentException(
                $"Not successful in getting results from this api {endPoint}. The response error was {errorResult}.");
        }

        private static Task<HttpClient> GetClientWithTokenAsync()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(BaseApi);
            return Task.FromResult(client);
        }
    }
}