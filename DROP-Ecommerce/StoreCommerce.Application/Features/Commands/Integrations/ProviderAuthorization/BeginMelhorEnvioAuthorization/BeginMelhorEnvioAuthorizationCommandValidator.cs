using SharedCommerce.Domain.Exceptions;

namespace StoreCommerce.Application.Features.Commands;

internal static class BeginMelhorEnvioAuthorizationCommandValidator
{
    public static void Validate(BeginMelhorEnvioAuthorizationCommand command)
    {
        if (!Enum.IsDefined(command.Environment))
            throw new SecurityFailure(400, "MelhorEnvio.InvalidEnvironment", "Ambiente do Melhor Envio inválido.");
    }
}
