using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using vp.validation;

namespace vp.services
{
    public class ValidationService : IValidationService
    {
        private readonly HttpClient client = new HttpClient();
        private bool isLoaded = false;
        private Dictionary<string, EntityValidator> validators;

        public async Task<bool> InitializeValidationService()
        {
            var rawValidators = await client.GetAsync($"{Config.BaseUrl}/assets/form-validations.json");
            var rawValidationTypes = await client.GetAsync($"{Config.BaseUrl}/assets/validation-types.json");

            var validationTypes = JsonConvert.DeserializeObject<List<string>>(await rawValidationTypes.Content.ReadAsStringAsync());
            var typeDefinitions = new Dictionary<string, dynamic>();
            var typeDefinitionasdfs = new Dictionary<string, ValidatorDefinition>();
            foreach (var validationType in validationTypes)
            {
                var rawTypeValidators = await client.GetAsync($"{Config.BaseUrl}/assets/{validationType}.json");
                var definition = JsonConvert.DeserializeObject<dynamic>(await rawTypeValidators.Content.ReadAsStringAsync());
                typeDefinitions[validationType] = definition;
            }

            var validatorDefinitions = JsonConvert.DeserializeObject<dynamic>(await rawValidators.Content.ReadAsStringAsync());
            EntityValidator validations = BuildValidations(validatorDefinitions, typeDefinitions);

            validators = new Dictionary<string, EntityValidator>();

            var uniqueRootKeys = validations.Keys.Select(key => key.Substring(0, key.IndexOf('.'))).Distinct();

            foreach(string uniqueKey in uniqueRootKeys)
            {
                validators[uniqueKey] = new EntityValidator();

                foreach(string validationKey in validations.Keys)
                {
                    var prefix = validationKey.Substring(0, validationKey.IndexOf('.'));
                    var newKey = validationKey.Substring(validationKey.IndexOf('.') + 1);

                    if(prefix == uniqueKey)
                    {
                        validators[uniqueKey][newKey] = validations[validationKey];
                    }
                }
            }

            isLoaded = true;
            return isLoaded;
        }


        public static EntityValidator BuildValidations(
            dynamic definitions,
            Dictionary<string, dynamic> typeDefinitions,
            string path = ""
        )
        {
            EntityValidator result = new EntityValidator();
            var prefix = path == "" ? "" : $"{path}.";


            foreach (var definition in definitions)
            {
                var validatorName = definition.Name;
                foreach (var fields in definition)
                {
                    foreach (var field in fields)
                    {
                        var fieldName = field.Name;

                        //Embedded type
                        if (fieldName == "type")
                        {
                            var type = field.Value.Value;
                            dynamic exo = new System.Dynamic.ExpandoObject();
                            ((IDictionary<String, Object>)exo).Add(validatorName, typeDefinitions[type]);

                            var poof = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(exo));
                            var subValidations = BuildValidations(poof, typeDefinitions, $"{prefix}");

                            foreach (var subKey in subValidations.Keys)
                            {
                                result[subKey] = subValidations[subKey];
                            }
                        }
                        else
                        {
                            foreach (var validations in field)
                            {
                                foreach (var validation in validations)
                                {
                                    var validationName = validation.Name;
                                    var validationValue = validation.Value.Value;

                                    if (validationName == "type")
                                    {
                                        var arrIndex = validationValue.IndexOf("[]");
                                        var typeDefinitionKey = arrIndex > 0 ? validationValue.Substring(0, arrIndex) : validationValue;

                                        dynamic exo = new System.Dynamic.ExpandoObject();
                                        ((IDictionary<String, Object>)exo).Add(fieldName, typeDefinitions[typeDefinitionKey]);
                                        var subDefinition = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(exo));
                                        var subValidations = BuildValidations(subDefinition, typeDefinitions, $"{prefix}{validatorName}");

                                        foreach(var subKey in subValidations.Keys)
                                        {
                                            result[subKey] = subValidations[subKey];
                                        }
                                    }
                                    else
                                    {
                                        result[$"{prefix}{validatorName}.{fieldName}.{validationName}"] = (value) =>
                                        {
                                            return ValidatorFunctions.ValidationFunctions[validationName](
                                                new string[] {
                                                value, $"{validationValue}"
                                                }
                                            );
                                        };
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        public static EntityValidator BuildValidations(
            ValidatorDictionaryDefinition definition,
            Dictionary<string, ValidatorDefinition> typeDefinitions,
            string path = "")
        {
            EntityValidator result = new EntityValidator();

            foreach (var validatorName in definition.Keys)
            {
                foreach (var fieldName in definition[validatorName].Keys)
                {
                    foreach (var validationName in definition[validatorName][fieldName].Keys)
                    {
                        if (validationName != "type")
                        {
                            var prefix = path == "" ? "" : $"{path}.";
                            result[$"{prefix}{validatorName}.{fieldName}.{validationName}"] = (value) => {
                                return ValidatorFunctions.ValidationFunctions[validationName](new string[] { value, definition[validatorName][fieldName][validationName] });
                            };
                        }
                        else
                        {
                            var subValidationDefinitions = new ValidatorDictionaryDefinition();
                            var validationType = definition[validatorName][fieldName][validationName];
                            subValidationDefinitions[fieldName] = typeDefinitions[validationType];
                            
                            var prefix = path == "" ? "" : $"{path}.";
                            var otherResult = BuildValidations(subValidationDefinitions, typeDefinitions, $"{prefix}{validatorName}");

                            foreach (var key in otherResult.Keys)
                            {
                                result[key] = otherResult[key];
                            }
                        }
                    }
                }
            }

            return result;
        }

        public async Task<Dictionary<string,string>> ValidateEntity(object entity, string validatorName) {
            if (!isLoaded) await InitializeValidationService();
            return ValidateEntity(entity, validators[validatorName]);
        }

        private Dictionary<string, string> ValidateEntity(
            object entity,
            EntityValidator validator
        ) 
        {
            var errors = new Dictionary<string, string>();
            var subValidations = new Dictionary<string, EntityValidator>();

            Type entityType = entity.GetType();
            var properties = entityType.GetProperties();

            foreach (var validatorKey in validator.Keys)
            {
                var seperatorIndex = validatorKey.IndexOf('.');

                var parts = validatorKey.Split('.');
                if (parts.Length == 2)
                {
                    var prop = entityType.GetProperty(parts[0]);
                    var value = prop.GetValue(entity);

                    var vType = prop.GetValue(entity).GetType();

                    string parameter = $"{value}";
                    if (!validator[validatorKey](parameter))
                    {
                        errors[validatorKey] = validatorKey;
                    }


                } else { 
                    var subPrefix = validatorKey.Substring(0, seperatorIndex);
                    var subIndex = validatorKey.Substring(seperatorIndex + 1);
                    if (!subValidations.ContainsKey(subPrefix)) subValidations.Add(subPrefix, new EntityValidator());

                    subValidations[subPrefix][subIndex] = validator[validatorKey];
                }
            }

            foreach(var subKey in subValidations.Keys)
            {
                var subType = entityType.GetProperty(subKey);
                if (subType.PropertyType.Name == "List`1")
                {
                    Type genericType = subType.PropertyType.GenericTypeArguments[0];
                    var list = subType.GetValue(entity) as IList;
                    for(var i = 0; i < list.Count; i++)
                    {
                        var subErrors = ValidateEntity(list[i], subValidations[subKey]);

                        foreach(var subErrorKey in subErrors.Keys)
                        {
                            errors.Add($"{subKey}[{i}].{subErrorKey}", subErrors[subErrorKey]);
                        }
                    }
                } else
                {
                    //TODO: Validate Embedded object that is not in an array
                    // (this scenario doesn't currently exist) 
                }
            }
            return errors;
        }
    }
}
