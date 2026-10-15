# Origem das fixtures do bloco de Pagamentos v1

- Repositório de origem: `portal-submodule-builder`, PR `portal-submodule-builder#164` (ainda NÃO mergeado).
  Se o #164 mudar na revisão, re-sincronize estes arquivos a partir do novo SHA, com o mesmo método.
- SHA: `843c51c6aedf3916973154ad67dbfa80dd515510` (branch `test/ipay-14-caracterizacao-bloco-builder`).
- Leitura só com `git show`/`git archive` desse SHA, extraindo para uma pasta vazia fora dos checkouts.
- Nada aqui roda Node em tempo de teste: os arquivos são JSON estáticos.

## Arquivos

| Arquivo | Origem no SHA | Método |
| --- | --- | --- |
| `checkout-v1.json` | `templates/builder/payment/features/referencias/checkout-v1.json` | cópia byte a byte (`git show <SHA>:<caminho>`, 34677 bytes) |
| `checkout-v1-acoes-saida.json` | `templates/builder/payment/state/Actions.ts`, `leavingPaymentActions(urlCheckout, urlAuth)` | gerado |
| `checkout-v1-conteudo.json` | `templates/builder/payment/state/Actions.ts`, `contentActionsPayment(textContent)` | gerado |
| `checkout-v1-outputs.json` | `templates/builder/payment/state/Outputs.ts`, `conditionOutputPayment` | gerado |
| `checkout-v1-estado.json` | `templates/builder/payment/state/State.ts`, `getPaymentState(...)` (`viewModel`), e `templates/builder/models/viewModels/StateViewModel.ts`, `StateViewModel.createModel` (`model`) | gerado |
| `configuracoes-v1.json` | formato de `templates/builder/payment/components/tabs/configuration/configuration.component.ts` (`updateSetVariableAction`) e `templates/builder/payment/components/tabs/paymentMethod/paymentMethod.component.ts` (`buildPaymentMethodData`) | criado nesta fase (formato transcrito, dados sintéticos) |
| `ORIGEM.md` | (este arquivo) | criado nesta fase |

## Como os arquivos gerados foram produzidos

1. `git -C portal-submodule-builder archive <SHA> templates/builder/models templates/builder/payment templates/builder/aiAgent templates/builder/services/BuilderStateService.ts templates/builder/services/BuilderAiAgentService.ts __mocks__ tsconfig.json package.json | tar -x` numa pasta vazia.
2. Bundle (esbuild) das funções puras, com os mesmos aliases do `jest.config.js` do SHA: `modules/application/detail/portal-submodule-builder/*` e `@detailssubmodule/*` apontam para a raiz; `types/CommandMethods`, `app.constants`, `fast-json-patch` e `find-duplicated-property-keys` apontam para `__mocks__/`. Os campos de classe usam `useDefineForClassFields`, que dá a mesma ordem de chaves do Babel do jest.
3. Substituições no bundle:
   - `uuid.v4()` devolve `"<id>"`;
   - `angular.copy` vira cópia JSON;
   - `BuilderStateService` vira só `PAYMENT_ID_PREFIX = 'payment:'` (linha 35 do arquivo no SHA);
   - `BuilderAiAgentService.isAiAgentState` devolve `false` (o `createModel` não o chama).
4. Parâmetros:
   - URLs `https://checkout.hmg.example.test` e `https://auth.hmg.example.test`, as mesmas da referência;
   - título `PaymentConstants.DEFAULT_BLOCK_TITLE` (`Payment`);
   - `textContent` = a chave `builder-tabs-content.media.userInput`. O portal passa a tradução dessa chave, que mora no blip-portal, fora do SHA.
5. Normalização igual à de `templates/builder/payment/features/__tests__/regressao.steps.test.ts`: `JSON.parse(JSON.stringify(x))` e todo `$id` trocado por `"<id>"`. Gravação com `JSON.stringify(x, null, 2)` mais `\n`.
6. Prova de fidelidade: o mesmo gerador reproduz `checkout-v1.json` byte a byte (`cmp` com o blob do SHA).

## Montagem do estado (`checkout-v1-estado.json`)

- `viewModel`: o `StateViewModel` de `getPaymentState`, como o editor guarda o bloco.
- `model`: o `State` publicado por `StateViewModel.createModel`, que é o formato executado pelo motor:
  - `inputActions` = ações de entrada (`checkout-v1.json`);
  - `input` = `input` do primeiro item de `checkout-v1-conteudo.json`;
  - `outputActions` = ações de saída (`checkout-v1-acoes-saida.json`);
  - `outputs` = `$conditionOutputs` com `stateId` `"self"` trocado pelo id do estado, mais o `$defaultOutput`, sem as saídas internas de pagamento sem destino.
- Referência a outro arquivo: `{"$fixture": "<arquivo>"}` é o conteúdo inteiro do arquivo desta pasta; `"$path"` é o caminho dentro dele, na sintaxe do `SelectToken` do Newtonsoft. O gerador confere que cada referência é igual ao valor que ela substituiu.
- O id do estado é `payment:<id>` e todo uuid vira `<id>`. Ids únicos são atribuídos em memória, se o motor exigir.
- `checkout-v1-outputs.json` é a lista crua de `Outputs.ts`, ainda com `stateId` `"self"`.

## Matriz de configurações (`configuracoes-v1.json`)

- Uma linha para cada valor da coluna `configuracao` de `Features/mensagens_do_bloco_v1.feature` (9), mais `checkout-link-e-pix` com `feedback: true`, usada pelo esquema de status.
- `configurationData`, `paymentMethodData` e `customizeData` são os objetos que o portal grava, com `JSON.stringify`, nos SetVariable `[SETUP][INIT] Configuration Data`, `[SETUP][INIT] Payment Method Data` e `[SETUP][INIT] Customize Data` (`PaymentDataService.updateVariableValue`). No fluxo de referência esses valores são `"{}"`; a montagem do fluxo troca cada um pelo objeto da linha, serializado.
- `State.ts` só declara `PaymentConfiguration` como `{ [key: string]: any }`: o formato real vem dos dois componentes citados na tabela.
- `configuracao`, `feedback` e `cartaoSalvo` são metadados do teste e não entram no fluxo. Em `checkout-link-com-cartao-salvo`, a configuração é igual à de `checkout-link`; o cartão salvo vem da resposta simulada (`hasSavedCard`) de `[CHECKOUT][EXEC] Get Customer Cards`.
- Só dados sintéticos:
  - chave PIX CPF `000.000.000-00` e chave aleatória `00000000-0000-0000-0000-000000000000`;
  - link em `pagamento.example.test`;
  - boleto com 47 zeros;
  - nenhum telefone, CNPJ, cartão ou e-mail real.
- No PIX do provedor checkout, o portal preencheria `keyType` `cnpj` e `pixKey` com o CNPJ do merchant (`fillPixFieldsFromMerchant`). Aqui esses campos ficam vazios, porque o script do provedor checkout usa a chave devolvida pela checkout-api.

## Itens de conferência da dona do repo

1. `configuracoes-v1.json`: o formato foi transcrito dos componentes, porque não há função pura que o gere. Confira campos e valores de cada linha.
2. Substituições do gerador: `PAYMENT_ID_PREFIX` transcrito e `textContent` igual à chave de tradução.
