Imports Newtonsoft.Json

''' <summary>
''' Represent a token response from the Web API service
''' </summary>
Public Class TokenModel
    <JsonProperty("access_token")>
    Public Property AccessToken() As String

    <JsonProperty("token_type")>
    Public Property TokenType() As String

    <JsonProperty("expires_in")>
    Public Property ExpiresIn() As TimeSpan?

    <JsonProperty("userName")>
    Public Property Username() As String

    <JsonProperty(".issued")>
    Public Property IssuedAt() As Date?

    <JsonProperty(".expires")>
    Public Property ExpiresAt() As Date?
End Class
