namespace CloudERP.Web.Models.Api
{
    public enum Response
    {
        SuccessfulRequest = 1,
        FailedRequest = 2,
        Authorized = 3,
        NotAuthorized = 4,
        UserNotExist = 5,
        NoResult = 6,
        UserIsAlreadyLoggedIn = 7,
        UserIsNotYetLoggedIn = 8,
    }
}
