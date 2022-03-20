using System.Text.Json;

namespace MemoRedis.API.Common
{
    public struct JsonResult<T>
    {
        public string JsonData { get; init; }

        #region ctor
        public JsonResult(T data!!, JsonSerializerOptions? options = null)
        {
            JsonData = JsonSerializer.Serialize(data, options);
        }
        #endregion

        #region Operator OverLoadings
        public static implicit operator JsonResult<T>(T data)
        {
            return new JsonResult<T>(data);
        }

        public static implicit operator T(JsonResult<T> jsonResult)
        {
            return JsonSerializer.Deserialize<T>(jsonResult.JsonData)!;
        }
        #endregion

    }
}