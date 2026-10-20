# WhatsApp Thread Control actions

This document describes two Builder actions available in the BLiP C# SDK for WhatsApp Thread Control in Meta's multi-partner model:

- `PassThreadControl`
- `TakeThreadControl`

## Responsibilities and boundaries

The SDK sends the operation, the current contact's `userIdentity` from the conversation context, and supported Builder action parameters configured by the bot developer.

The WhatsApp Gateway (Channels application) enriches and sends WhatsApp/Meta-specific data (for example: phone number id, partner app id, Graph API payload shape, and tokens).

The resource uses `controlType` and `userIdentity`, not `type` or `identity`. A configured `identity` does not override the context identity.

## Command contract sent to Channels

Both actions send a LIME command with:

- `method`: `set`
- `type`: `application/vnd.iris.whatsapp.thread-control+json`
- `to`: `postmaster@wa.gw.msging.net`
- `uri`: `/thread-control`
- `id`: generated via `EnvelopeId.NewId()`
- `resource`: `controlType`, `userIdentity`, optional `targetRole`, and optional `metadata`

The operation is selected by `resource.controlType`: `pass`, `release`, or `take`. Null optional fields are omitted; empty metadata strings are preserved.

If Channels returns `Failure`, the action throws `InvalidOperationException` with the failure code and description, or `No reason returned` if no reason is supplied. Sender exceptions and cancellation propagate to the caller.

## PassThreadControl

Passes thread control from BLiP to a target partner, or releases control.

### Settings

| Property | Type | Required | Notes |
|---|---|---|---|
| `controlType` | string | No | `pass` (default) or `release`. Use `TakeThreadControl` for `take`. |
| `targetRole` | string | For `pass` | Supported wire values: `customer_service`, `marketing`, `utility`, `ctwa`, `ai_agent`, `escalation`. Must be omitted or null for `release`. |
| `metadata` | string | No | Value is expected to be resolved by FlowManager before action execution. |

### Validation

- Passing without a target role, releasing with a target role, or using `take` throws `ValidationException` before sending.
- Unrecognized enum strings for `controlType` or `targetRole` fail deserialization with `JsonSerializationException` before sending.
- Unknown settings are not forwarded to the command resource.

### Resource shape

```json
{
  "controlType": "pass",
  "userIdentity": "customer@wa.gw.msging.net",
  "targetRole": "customer_service",
  "metadata": "<optional, resolved>"
}
```

For release, `controlType` is `release` and `targetRole` is omitted.

## TakeThreadControl

Takes thread control back to BLiP, then redirects the conversation to a router service after the gateway command succeeds.

### Settings

| Property | Type | Required | Notes |
|---|---|---|---|
| `service` | string | Yes | Target service address for the redirect. Not forwarded in the thread control resource. |
| `message` | string | No | If nonblank, sent as `text/plain` redirect context. Not forwarded in the thread control resource. |
| `metadata` | string | No | Value is expected to be resolved by FlowManager before action execution. |

### Validation

- Missing, empty, or whitespace-only `service` throws `ValidationException` before sending.
- The operation is always `take`. Settings such as `targetRole` and `controlType` are ignored and do not override the resource.
- Unknown settings are not forwarded to the command resource.
- No redirect is attempted if the gateway command fails or is canceled. Redirect exceptions propagate to the caller.

### Resource shape

```json
{
  "controlType": "take",
  "userIdentity": "customer@wa.gw.msging.net",
  "metadata": "<optional, resolved>"
}
```
