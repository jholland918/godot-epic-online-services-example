using Godot;
using System;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Logging;
using Epic.OnlineServices.Platform;
using Epic.OnlineServices.UserInfo;

public class Eos
{
    private PlatformInterface _platform;
    private UserInfoInterface _userInfo;

    public void Initialize()
    {
        var config = new ConfigFile();
        Error error = config.Load("res://eos.cfg"); // not checked into source control
        if (!error.HasFlag(Error.Ok))
        {
            throw new Exception("Probled loading eos.cfg");
        }

        // Set these values as appropriate. For more information, see the Developer Portal documentation.
        string productName = "MyCSharpApplication";
        string productVersion = "1.0";
        string productId = config.GetValue("main", "product_id").ToString();
        string sandboxId = config.GetValue("main", "sandbox_id").ToString();
        string deploymentId = config.GetValue("main", "deployment_id").ToString();
        string clientId = config.GetValue("main", "client_id").ToString();
        string clientSecret = config.GetValue("main", "client_secret").ToString();

        var initializeOptions = new InitializeOptions()
        {
            ProductName = productName,
            ProductVersion = productVersion
        };

        var initializeResult = PlatformInterface.Initialize(initializeOptions);
        if (initializeResult != Result.Success)
        {
            throw new Exception("Failed to initialize platform: " + initializeResult);
        }

        // The SDK outputs lots of information that is useful for debugging.
        // Make sure to set up the logging interface as early as possible: after initializing.
        LoggingInterface.SetLogLevel(LogCategory.AllCategories, LogLevel.VeryVerbose);
        LoggingInterface.SetCallback((LogMessage logMessage) =>
        {
            Console.WriteLine(logMessage.Message);
        });

        var options = new Options()
        {
            ProductId = productId,
            SandboxId = sandboxId,
            DeploymentId = deploymentId,
            ClientCredentials = new ClientCredentials()
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            }
        };

        _platform = PlatformInterface.Create(options);
        if (_platform == null)
        {
            throw new Exception("Failed to create platform");
        }

        _userInfo = _platform.GetUserInfoInterface();
    }

    public void Login(Action<string> onLogin)
    {
        // See https://dev.epicgames.com/docs/services/en-US/DevPortal/IdentityProviderManagement/index.html
        var loginCredentialType = LoginCredentialType.AccountPortal;
        /// These fields correspond to <see cref="Credentials.Id" /> and <see cref="Credentials.Token" />,
        /// and their use differs based on the login type. For more information, see <see cref="Credentials" />
        /// and the Auth Interface documentation.
        string loginCredentialId = "";
        string loginCredentialToken = "";

        var authInterface = _platform.GetAuthInterface();
        if (authInterface == null)
        {
            throw new Exception("Failed to get auth interface");
        }

        var loginOptions = new LoginOptions()
        {
            Credentials = new Credentials()
            {
                Type = loginCredentialType,
                Id = loginCredentialId,
                Token = loginCredentialToken,
                ExternalType = ExternalCredentialType.Epic
            }
        };

        // Ensure platform tick is called on an interval, or the following call will never callback.
        authInterface.Login(loginOptions, null, (LoginCallbackInfo loginCallbackInfo) =>
        {
            if (loginCallbackInfo.ResultCode == Result.Success)
            {
                Console.WriteLine("Login succeeded");
                onLogin("Login succeeded, getting user info...");

                var userInfoOptions = new CopyUserInfoOptions
                {
                    LocalUserId = loginCallbackInfo.LocalUserId,
                    TargetUserId = loginCallbackInfo.LocalUserId
                };

                _userInfo.CopyUserInfo(userInfoOptions, out UserInfoData userInfoData);

                onLogin($"User: {userInfoData.DisplayName}");
            }
            else if (Common.IsOperationComplete(loginCallbackInfo.ResultCode))
            {
                Console.WriteLine("Login failed: " + loginCallbackInfo.ResultCode);
                onLogin("Login failed: " + loginCallbackInfo.ResultCode);
            }
        });
    }

    public void Tick()
    {
        _platform.Tick();
    }

    public void Shutdown()
    {
        _platform.Release();

        var shutdownResult = PlatformInterface.Shutdown();
        if (shutdownResult != Result.Success)
        {
            throw new Exception("Failed to shutdown platform: " + shutdownResult);
        }
    }
}
