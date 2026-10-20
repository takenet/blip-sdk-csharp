# WhatsApp Thread Control actions

This document describes two Builder actions available in the BLiP C# SDK for WhatsApp Thread Control in Meta's multi-partner model:

- `PassThreadControl`
- `TakeThreadControl`

## Responsibilities and boundaries

The SDK sends only Builder action parameters configured by the bot developer.

The WhatsApp Gateway (Channels application) enriches and sends WhatsApp/Meta-specific data (for example: customer identity, phone number id, partner app id, Graph API payload shape, and tokens).

Because of that, these actions must **not** send fields such as `identity`, `type`, or `controlType` in the command resource.

## Command contract sent to Channels

Both actions send a LIME command with:

- `method`: `set`
- `type`: `application/json`
- `to`: `postmaster@wa.gw.msging.net` (confirm with Channels if this contract changes)
- `id`: generated via `EnvelopeId.NewId()`
- `resource`: only user-configurable fields, omitting null/empty values

Operation-specific URIs:

- Pass thread control: `/thread-control/pass`
- Take thread control: `/thread-control/take`

If Channels returns `Failure`, the action throws and surfaces the failure reason.

## PassThreadControl

Passes thread control from BLiP to the next app/partner.

### Settings

| Property | Type | Required | Notes |
|---|---|---|---|
| `targetRole` | string | No | Must be exactly one of: `customer_service`, `marketing`, `utility`, `ctwa`, `ai_agent`, `escalation` (case-sensitive, snake_case). If omitted, Channels/Meta can follow the default queue behavior. |
| `metadata` | string | No | Value is expected to be resolved by FlowManager before action execution. |

### Validation

- If `targetRole` is provided and invalid, a `ValidationException` is thrown before sending.
- Unknown settings are not forwarded to the command resource.

### Resource shape

```json
{
  "targetRole": "<optional>",
  "metadata": "<optional, resolved>"
}
```

## TakeThreadControl

Takes thread control back to BLiP.

### Settings

| Property | Type | Required | Notes |
|---|---|---|---|
| `metadata` | string | No | Value is expected to be resolved by FlowManager before action execution. |

### Validation

- `targetRole` is not allowed for this action. If present, a `ValidationException` is thrown before sending.
- Unknown settings are not forwarded to the command resource.

### Resource shape

```json
{
  "metadata": "<optional, resolved>"
}
```
