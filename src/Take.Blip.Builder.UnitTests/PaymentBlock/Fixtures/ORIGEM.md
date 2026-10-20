# Origin of the Payments block v1 fixtures

- Source repository: `portal-submodule-builder`, PR `portal-submodule-builder#164`, merged on 2026-10-19 into `main`.
- SHA: `df193fcd7590c85b70140c424f32ccef3d22d681` (squash commit of #164 on `main`). The files were first produced from the PR head `843c51c6aedf3916973154ad67dbfa80dd515510` and come out identical from `df193fc`.
- If these source files change on `main`, re-sync the fixtures from the new SHA, with the same method.
- Read only with `git show`/`git archive` of that SHA, extracting to an empty folder outside the checkouts.
- Nothing here runs Node at test time: the files are static JSON.

## Files

| File | Origin in the SHA | Method |
| --- | --- | --- |
| `checkout-v1.json` | `templates/builder/payment/features/references/checkout-v1.json` | byte-for-byte copy (`git show <SHA>:<path>`, 34677 bytes) |
| `checkout-v1-acoes-saida.json` | `templates/builder/payment/state/Actions.ts`, `leavingPaymentActions(urlCheckout, urlAuth)` | generated |
| `checkout-v1-conteudo.json` | `templates/builder/payment/state/Actions.ts`, `contentActionsPayment(textContent)` | generated |
| `checkout-v1-outputs.json` | `templates/builder/payment/state/Outputs.ts`, `conditionOutputPayment` | generated |
| `checkout-v1-estado.json` | `templates/builder/payment/state/State.ts`, `getPaymentState(...)` (`viewModel`), and `templates/builder/models/viewModels/StateViewModel.ts`, `StateViewModel.createModel` (`model`) | generated |
| `configuracoes-v1.json` | format from `templates/builder/payment/components/tabs/configuration/configuration.component.ts` (`updateSetVariableAction`) and `templates/builder/payment/components/tabs/paymentMethod/paymentMethod.component.ts` (`buildPaymentMethodData`) | created in this phase (transcribed format, synthetic data) |
| `ORIGEM.md` | (this file) | created in this phase |

## How the generated files were produced

1. `git -C portal-submodule-builder archive <SHA> templates/builder/models templates/builder/payment templates/builder/aiAgent templates/builder/services/BuilderStateService.ts templates/builder/services/BuilderAiAgentService.ts __mocks__ tsconfig.json package.json | tar -x` into an empty folder.
2. Bundle (esbuild) of the pure functions, with the same aliases as the `jest.config.js` in the SHA: `modules/application/detail/portal-submodule-builder/*` and `@detailssubmodule/*` point to the root; `types/CommandMethods`, `app.constants`, `fast-json-patch` and `find-duplicated-property-keys` point to `__mocks__/`. Class fields use `useDefineForClassFields`, which gives the same key order as the jest Babel setup.
3. Replacements in the bundle:
   - `uuid.v4()` returns `"<id>"`;
   - `angular.copy` becomes a JSON copy;
   - `BuilderStateService` becomes only `PAYMENT_ID_PREFIX = 'payment:'` (line 35 of the file in the SHA);
   - `BuilderAiAgentService.isAiAgentState` returns `false` (`createModel` does not call it).
4. Parameters:
   - URLs `https://checkout.hmg.example.test` and `https://auth.hmg.example.test`, the same as in the reference;
   - title `PaymentConstants.DEFAULT_BLOCK_TITLE` (`Payment`);
   - `textContent` = the key `builder-tabs-content.media.userInput`. The portal passes the translation of that key, which lives in blip-portal, outside the SHA.
5. Normalization equal to the one in `templates/builder/payment/features/__tests__/regression.steps.test.ts`: `JSON.parse(JSON.stringify(x))` and every `$id` replaced with `"<id>"`. Written with `JSON.stringify(x, null, 2)` plus `\n`.
6. Fidelity proof: the same generator reproduces `checkout-v1.json` byte for byte (`cmp` against the blob in the SHA).

## State assembly (`checkout-v1-estado.json`)

- `viewModel`: the `StateViewModel` from `getPaymentState`, as the editor stores the block.
- `model`: the `State` published by `StateViewModel.createModel`, which is the format the engine runs:
  - `inputActions` = entering actions (`checkout-v1.json`);
  - `input` = `input` of the first item of `checkout-v1-conteudo.json`;
  - `outputActions` = leaving actions (`checkout-v1-acoes-saida.json`);
  - `outputs` = `$conditionOutputs` with `stateId` `"self"` replaced by the state id, plus `$defaultOutput`, without the internal payment outputs that have no destination.
- Reference to another file: `{"$fixture": "<file>"}` is the whole content of the file in this folder; `"$path"` is the path inside it, in Newtonsoft `SelectToken` syntax. The generator checks that each reference equals the value it replaced.
- The state id is `payment:<id>` and every uuid becomes `<id>`. Unique ids are assigned in memory, if the engine requires them.
- `checkout-v1-outputs.json` is the raw list from `Outputs.ts`, still with `stateId` `"self"`.

## Configuration matrix (`configuracoes-v1.json`)

- One row for each configuration of `PaymentBlockV1MessageTests.OrderMessageShouldMatchApprovedReference` (9), plus `checkout-link-e-pix` with `feedback: true`, used by `PaymentBlockV1MessageTests.StatusMessageShouldMatchApprovedReference`.
- `configurationData`, `paymentMethodData` and `customizeData` are the objects the portal writes, with `JSON.stringify`, to the SetVariable actions `[SETUP][INIT] Configuration Data`, `[SETUP][INIT] Payment Method Data` and `[SETUP][INIT] Customize Data` (`PaymentDataService.updateVariableValue`). In the reference flow these values are `"{}"`; the flow assembly replaces each one with the serialized object of the row.
- `State.ts` only declares `PaymentConfiguration` as `{ [key: string]: any }`: the real format comes from the two components cited in the table.
- `configuracao`, `feedback` and `cartaoSalvo` are test metadata and do not go into the flow. In `checkout-link-com-cartao-salvo`, the configuration is the same as `checkout-link`; the saved card comes from the simulated response (`hasSavedCard`) of `[CHECKOUT][EXEC] Get Customer Cards`.
- Synthetic data only:
  - CPF PIX key `000.000.000-00` and random key `00000000-0000-0000-0000-000000000000`;
  - link at `pagamento.example.test`;
  - bank slip with 47 zeros;
  - no real phone number, CNPJ, card or e-mail.
- For the checkout provider PIX, the portal would fill `keyType` `cnpj` and `pixKey` with the merchant CNPJ (`fillPixFieldsFromMerchant`). Here those fields stay empty, because the checkout provider script uses the key returned by the checkout-api.

## Review items for the repository owner

1. `configuracoes-v1.json`: the format was transcribed from the components, because there is no pure function that generates it. Check the fields and values of each row.
2. Generator replacements: `PAYMENT_ID_PREFIX` transcribed and `textContent` equal to the translation key.
