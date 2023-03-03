using System;
using System.Collections.Generic;

namespace vp.validation
{
    public static class ValidatorFunctions
    {
        public static readonly Dictionary<string, Func<string[], bool>> ValidationFunctions = new Dictionary<string, Func<string[], bool>>
        {
            { 
                "required", value => {
                    return value[0] != null && value[0].Trim() != ""; 
                }   
            },
            {
                "max-length",
                value => {
                    try
                    {
                        var target = double.Parse(value[1]);
                        return value[0].Length <= target;
                    } catch
                    {
                        return false;
                    }
                }
            },
            {
                "gt",
                value =>
                {
                    try
                    {
                        var parsedValue = double.Parse(value[0]);
                        var target = double.Parse(value[1]);
                        return parsedValue > target;
                    } catch
                    {
                        return false;
                    }

                }
            },
            {
                "gte",
                value =>
                {
                    try
                    {
                        var parsedValue = double.Parse(value[0]);
                        var target = double.Parse(value[1]);
                        return parsedValue >= target;
                    } catch
                    {
                        return false;
                    }

                }
            },
            {
                "lt",
                value =>
                {
                    try
                    {
                        var parsedValue = double.Parse(value[0]);
                        var target = double.Parse(value[1]);
                        return parsedValue < target;
                    } catch
                    {
                        return false;
                    }

                }
            },
            {
                "lte",
                value =>
                {
                    try
                    {
                        var parsedValue = double.Parse(value[0]);
                        var target = double.Parse(value[1]);
                        return parsedValue < target;
                    } catch
                    {
                        return false;
                    }

                }
            }
        };
    }
}
