using Moveness.DTOS.Enums;
using System.Collections.Generic;
using System.Linq;

namespace Moveness.DTOS.ResponseObjects
{
    public class BadRequestResponse
    {
        public BadRequestCode Code { get; set; }

        public static BadRequestResponse FromIdentity(IEnumerable<string> codes)
        {
            var code = codes.FirstOrDefault();

            var response = code switch
            {
                "InvalidUserName" => BadRequestCode.InvalidUserName,
                "InvalidEmail" => BadRequestCode.InvalidEmail,
                "DuplicateUserName" => BadRequestCode.DuplicateUserName,
                "DuplicateEmail" => BadRequestCode.DuplicateEmail,
                "UserAlreadyHasPassword" => BadRequestCode.UserAlreadyHasPassword,
                "PasswordTooShort" => BadRequestCode.PasswordTooShort,
                "PasswordRequiresUniqueChars" => BadRequestCode.PasswordRequiresUniqueChars,
                "PasswordRequiresNonAlphanumeric" => BadRequestCode.PasswordRequiresNonAlphanumeric,
                "PasswordRequiresDigit" => BadRequestCode.PasswordRequiresDigit,
                "PasswordRequiresLower" => BadRequestCode.PasswordRequiresLower,
                "PasswordRequiresUpper" => BadRequestCode.PasswordRequiresUpper,
                "InvalidToken" => BadRequestCode.InvalidToken,
                _ => BadRequestCode.UnidentifiedIdentityError,
            };

            return new BadRequestResponse { Code = response };
        }
    }
}
