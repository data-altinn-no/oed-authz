# oed-auzth
ASP.NET Core Web API handling events for OED/DD roles, persisting them and providing av PIP API for Altinn Authorization. This also exposes an API for external consumers requiring Maskinporten-authentication.

See https://oed-test-authz-app.azurewebsites.net/swagger/ for API documentation.


## Using the API for external consumers (banks etc.)

External consumers should use `/api/v1/authorization/roles`. This endpoint requires a Maskinporten-token with one of the following scopes;`altinn:dd:authlookup:probateonly` or `altinn:dd:authlookup:allroles`. Requests with the former will only return role assignments for the role `urn:digitaltdodsbo:skifteattest`, while the latter will return all roles.

### Example

Requests must contain a `Authorization`-header with a Maskinporten-token using the `Bearer` scheme. The request body must be a JSON object with `estateSsn` and `recipientSsn` properties, both being 11-digit norwegian identification numbers for the deceased (estate) and heir (recipient), respectively.

```jsonc
// POST https://oed-test-authz-app.azurewebsites.net/api/v1/authorization/roles
{
    "estateSsn": "11111111111",
    "recipientSsn": "22222222211"
}
```

Response:
```json
{
    "estateSsn": "11111111111",
    "recipientSsn": "22222222211",
    "roleAssignments": [
        {
            "role": "urn:digitaltdodsbo:skifteattest",
            "created": "2023-02-20T10:00:06.401416+00:00"
        }
    ]
}
```

If no relation (ie. role assignement) exists, an empty `roleAssignments` array will be returned.

## Internal usage

Internal (platform and DD application) consumers should use the `/api/v1/pip` endpoint. This requires a platform-token where the `urn:altinn:app` claim is set to `platform.authorization`. Supply a `PipRequest`-body with one or both the `from` and `to` properties set to norwegian identification numbers for the deceased (estate) and heir (recipient), respectively.


## Local development setup

1. Install PostgreSQL 13 or later
2. Install pgAdmin
4. Create a database locally with name `oedauthz`
3. Create the user `oedpgadmin` (only used for migrations), set password to `secret`. Give all privileges to `oedauthz`
4. Create the user `oedpguser`, set password to `secret`. Give usage privileges to `oedauthz`.
5. Run/debug the project

This should build and migrate the database. Open https://localhost/swagger for Swagger UI.
