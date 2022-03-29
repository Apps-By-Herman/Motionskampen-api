namespace Moveness.DTOS.Enums
{
    public enum BadRequestCode
    {
        UnidentifiedIdentityError = 0,
        IdsNotMatching = 1,
        InvalidUserName = 2,
        InvalidEmail = 3,
        DuplicateUserName = 4,
        DuplicateEmail = 5,
        UserAlreadyHasPassword = 6,
        PasswordTooShort = 7,
        PasswordRequiresUniqueChars = 8,
        PasswordRequiresNonAlphanumeric = 9,
        PasswordRequiresDigit = 10,
        PasswordRequiresLower = 11,
        PasswordRequiresUpper = 12,
        InvalidToken = 13,
        CanNotDeleteAcceptedChallenge = 14,
        CanNotChallengeYourself = 15,
        CanNotEditChallengeAfterAcceptance = 16,
        UserAlreadyInTeam = 17,
        CanNotDeleteTeamWhenActiveChallenges = 18,
        UserIsOwnerOfTeam = 19
    };
}
