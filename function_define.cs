using System.Collections;

namespace GoViewServer
{
    public class function_define
    {
        public static ArrayList sort_arraylist(ArrayList arrayList)
        {
            return new ArrayList(
                arrayList.Cast<Dictionary<string, object>>()
                    .OrderBy(dict => GetSafePageNumber(dict))
                    .ToList()
            );
        }

        private static int GetSafePageNumber(Dictionary<string, object> dict)
        {
            try
            {
                // 1. 尝试直接获取int类型
                if (dict.TryGetValue("page_number", out object value) && value is int intValue)
                {
                    return intValue;
                }

                // 2. 尝试获取可转换为int的类型（string/long/double等）
                if (dict.TryGetValue("page_number", out object convertibleValue) &&
                    convertibleValue != null &&
                    int.TryParse(convertibleValue.ToString(), out int parsedInt))
                {
                    return parsedInt;
                }

                // 3. 处理非常规数值类型（如double 1.5 → 1）
                if (dict.TryGetValue("page_number", out object numericValue) &&
                    numericValue is IConvertible convertible)
                {
                    return Convert.ToInt32(convertible.ToDouble(null));
                }
            }
            catch
            {
                return 0;
            }

            // 异常处理策略：返回int.MaxValue将无效条目放在末尾
            return 9999;
        }
    }
}
