# oed-auzth
ASP.NET Core Web API handling events for OED/DD roles, persisting them and providing av PIP API for Altinn Authorization. This also exposes an API for external consumers requiring Maskinporten-authentication.

See https://oed-test-authz-app.azurewebsites.net/swagger/ for API documentation.

## Using the API for external consumers (banks etc.)

External consumers should use `/api/v1/authorization/roles`. This endpoint requires a Maskinporten-token with the scope; `altinn:dd:authlookup`. The following role codes will be made available

* `urn:digitaltdodsbo:formuesfullmakt` (granted by the courts)
* `urn:digitaltdodsbo:skifteattest` (heir who has assumed debt responsibility, granted by the courts)
* `urn:digitaltdodsbo:skiftefullmakt` (proxy appointed by all heirs with a probate certificate)

Other scopes might be added

### Example

Requests must contain a `Authorization`-header with a Maskinporten-token using the `Bearer` scheme. The request body must be a JSON object with `estateSsn`. Optionally, a `recipientSsn` property can be supplied. Both must be 11-digit norwegian identification numbers for the deceased (estate) and heir (recipient), respectively.

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
    "roleAssignments": [
        {
            "estateSsn": "11111111111",
            "recipientSsn": "22222222211",
            "role": "urn:digitaltdodsbo:skifteattest",
            "created": "2023-02-20T10:00:06.401416+00:00"
        }
    ]
}
```

If no relation (ie. role assignement) exists, an empty `roleAssignments` array will be returned.

## Internal Altinn usage 

Supply a `PipRequest`-body with one or both the `from` and `to` properties set to norwegian identification numbers for the deceased (estate) and heir (recipient), respectively to the endpoint `/api/v1/pip`. One of the parameters can be omitted to get a list of all relations for the given from/to. This will include additional roles compared to the API for external consumers.

This requires a Maskinporten-token with the scope `altinn:dd:internal`

### Example

```jsonc
// POST https://oed-test-authz-app.azurewebsites.net/api/v1/pip
{
    "from": "11111111111"
    // "to" is omitted
}
```

Response:
```json
{
    "roleAssignments": [
        {
            "urn:oed:rolecode": "urn:digitaltdodsbo:formuesfullmakt",
            "from": "11111111111",
            "to": "22222222211",
            "created": "2023-02-20T10:00:06.401416+00:00"
        },
        {
            "urn:oed:rolecode": "urn:digitaltdodsbo:arving:ektefelleEllerPartner",
            "from": "11111111111",
            "to": "22222222211",
            "created": "2023-02-20T10:00:06.401416+00:00"
        },
        {
            "urn:oed:rolecode": "urn:digitaltdodsbo:skifteattest",
            "from": "11111111111",
            "to": "22222222211",
            "created": "2023-02-20T10:00:06.401416+00:00"
        }
    ]
}
```


## Local development setup

1. Install PostgreSQL 13 or later
2. Install pgAdmin
4. Create a database locally with name `oedauthz`
3. Create the user `oedpgadmin` (only used for migrations), set password to `secret`. Give all privileges to `oedauthz`
4. Create the user `oedpguser`, set password to `secret`. Give usage privileges to `oedauthz`.
5. Run/debug the project

This should build and migrate the database. Open https://localhost/swagger for Swagger UI.
