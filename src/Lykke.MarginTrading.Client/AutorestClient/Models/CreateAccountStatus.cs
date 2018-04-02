// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Lykke.MarginTrading.Client.AutorestClient.Models
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System.Runtime;
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines values for CreateAccountStatus.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CreateAccountStatus
    {
        [EnumMember(Value = "Available")]
        Available,
        [EnumMember(Value = "Created")]
        Created,
        [EnumMember(Value = "Error")]
        Error
    }
    internal static class CreateAccountStatusEnumExtension
    {
        internal static string ToSerializedValue(this CreateAccountStatus? value)
        {
            return value == null ? null : ((CreateAccountStatus)value).ToSerializedValue();
        }

        internal static string ToSerializedValue(this CreateAccountStatus value)
        {
            switch( value )
            {
                case CreateAccountStatus.Available:
                    return "Available";
                case CreateAccountStatus.Created:
                    return "Created";
                case CreateAccountStatus.Error:
                    return "Error";
            }
            return null;
        }

        internal static CreateAccountStatus? ParseCreateAccountStatus(this string value)
        {
            switch( value )
            {
                case "Available":
                    return CreateAccountStatus.Available;
                case "Created":
                    return CreateAccountStatus.Created;
                case "Error":
                    return CreateAccountStatus.Error;
            }
            return null;
        }
    }
}
