using CI.HttpClient;
using UnityEngine;

public class CubeJumpHTTPClient
{
    private static CubeJumpHTTPClient instance;

    public HttpClient client;
    public string baseUrl = "https://briser-games-server.onrender.com/";

    public static CubeJumpHTTPClient GetInstance()
    {
        instance ??= new CubeJumpHTTPClient();

        return instance;
    }

    private CubeJumpHTTPClient()
    {
        client = new HttpClient();
    }

    public AuthorizationRoutes GetAuthorizationRoutes()
    {
        return AuthorizationRoutes.GetInstance(this);
    }

    public PlayerRoutes GetPlayerRoutes()
    {
        return PlayerRoutes.GetInstance(this);
    }
}
