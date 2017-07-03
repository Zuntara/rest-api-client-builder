# REST API Client Builder for .NET

This tool will execute web requests to a given REST api and takes care of the formatting of the client of you

# How to install

You can install this package through NuGet:

**Install-Package RestApiClientBuilder.Core**

# Quick start

## 1. Define an endpoint definition to describe your REST API

    public class EndpointDefinitions
    {
        [ThreadStatic] private static readonly Uri BaseAddressRestService;

        static EndpointDefinitions()
        {
            // Fetch the configuration from the config file
            BaseAddressRestService = new Uri(AppSettingsHelper.GetValue<string>("WebApiBaseAddress"));
        }

        public class Users
        {
            private static string ControllerName => "Users";

            /// <summary>
            /// Searches users
            /// </summary>
            public static EndpointDefinition Search = EndpointDefinition.Build(BaseAddressRestService, ControllerName, "Search");

            /// <summary>
            /// Gets the details for a user
            /// </summary>
            public static EndpointDefinition UnLoadingPoints = EndpointDefinition.Build(BaseAddressRestService, ControllerName, "Details");
            
            /// <summary>
            /// Update a user, given an id in the uri
            /// </summary>
            public static EndpointDefinition Update = EndpointDefinition.Build(BaseAddressRestService, ControllerName, "Update/{id}");
        }
    }

## 2. Wrap your call in a service of some sort and execute it

For example:

    public class MyUserService : IMyUserService
    {
        public async Task<UserDto[]> SearchUserAsync(UserSearchCriteriaDto searchCriteria)
        {
            RestApiCallResult result = await EndpointDefinitions.Users.Search.Get()
                .WithQueryArgument("model", searchCriteria)
                .ExecuteAsync();

            Debug.Writeline($"Call {result.Uri} took {result.Speed}");

            if (result.IsSucceeded)
            {
                return result.Parse<UserDto[]>();
            }

            Debug.Writeline(result.GetErrorMessage());
            
            /* Do error handling here */

            return new UserDto[0];
        }
    }

# Possible actions

You can execute a call in two ways.
* From the **RestApiClientBuilder** class
* From the **EndpointDefinition** class

## Syntax

Create a Builder first with

    RestApiClientBuilder.Build()

Define optional behavior(s)
  Example for OAuth2 security:
  
  The OAuth2 package **Install-Package RestApiClientBuilder.OAuth2** does include behaviors that can handle OAuth2 calls on the client.

      .Behavior(IRestBehavior)
    
Define the description of the call

    .From(definition, [timeoutInMilliseconds])

Define the Method of the call

    .Get()
    .Delete()
    .Put(object)
    .Post(object)

Define arguments of the call

    .WithUriArgument( name_of_argument , value_of_argument )
    
    .WithQueryArgument( [variableName], [object] )
    
Define optional error handling

    .OnErrorResponse(httpCode => { /* handle failed call */ })
    
Execute the call

    .ExecuteAsync()
    
Returns an object of **RestApiCallResult**
This object contains the Speed and status of the call

        /// <summary>
        /// List of optional errors when they occured.
        /// </summary>
        public List<string> Errors { get; set; }

        /// <summary>
        /// Indication that all went well
        /// </summary>
        public bool IsSucceeded { get; set; }

        /// <summary>
        /// Contains the JSON retrieved from the call
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// The URI that is requested
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// Gets the speed of the call
        /// </summary>
        public TimeSpan Speed { get; set; }
        
