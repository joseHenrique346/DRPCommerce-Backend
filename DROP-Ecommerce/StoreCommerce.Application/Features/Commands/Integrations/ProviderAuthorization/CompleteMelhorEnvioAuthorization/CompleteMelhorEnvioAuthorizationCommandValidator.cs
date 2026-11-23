namespace StoreCommerce.Application.Features.Commands;

internal static class CompleteMelhorEnvioAuthorizationCommandValidator
{
    public static bool HasValidState(CompleteMelhorEnvioAuthorizationCommand command)
    {
        return Enum.IsDefined(command.Environment) &&
            command.State.Length is >= 20 and <= 200 &&
            command.BrowserBinding.Length is >= 20 and <= 200;
    }
}
