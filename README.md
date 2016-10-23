# DelSole.ClientAuth

![](https://img.shields.io/badge/release-stable-brightgreen.svg)  ![](https://img.shields.io/badge/NuGet-v1.0-blue.svg)

The DelSole.ClientAuth portable library provides an easy way to authenticate users against a Web API service and to send authenticated GET, POST, PUT and DELETE requests. At this time, only Bearer authentication is supported.

**Usage**

Create an instance of the AuthService class passing the Web API base address as the parameter.

Invoke LoginAsync passing username and password. This returns an instance of the TokenModel class

The TokenModel class represents the access token information, including the token string, issuance and expiration date.

After you get a token, invoke GetAsync, PostAsync<T>, PutAsync<T>, and DeleteAsync to perform CRUD operations over the specified Web API controller.

These methods search for the token stored inside the AuthService class, and they raise an UnauthorizedException if no token is found.

**Supported platforms**

This is a portable libraries that can be used anywhere, including Xamarin apps.
