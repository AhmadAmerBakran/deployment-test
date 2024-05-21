using System.ComponentModel.DataAnnotations;
using lib;

namespace Api.Dtos;

public class ClientWantsToControlCarDto : BaseDto
{
    public string Topic { get; set; }
    public string Command { get; set; }
}

public class ClientWantsToReceiveNotificationsDto : BaseDto
{
}

public class ServerSendsErrorMessageToClientDto : BaseDto
{
    public string? ErrorMessage { get; set; }
}


public class ClientWantsToSignInDto: BaseDto
{
    [Required(ErrorMessage = "Username is required.")]
    [MinLength(2, ErrorMessage = "Username must be at least 2 characters long.")]
    [MaxLength(25, ErrorMessage = "Username must not exceed 25 characters.")]
    public string NickName { get; set; }
}

public class ClientWantsToSignOutDto : BaseDto
{
    
}

public class ServerClientSignIn : BaseDto
{
    public string Message { get; set; }
}

public class ClientWantsToGetCarLogDto : BaseDto
{
    
}