# Blip Builder — Monitoring Events

Documentação de todos os eventos gerados pelo `IBlipLogger` no pipeline do Builder.

---

## Estrutura comum (LogInput)

Todos os eventos compartilham os seguintes campos de nível raiz no `LogInput`:

| Campo | Tipo | Descrição |
|---|---|---|
| `Title` | `string` | Nome do evento (ver tabelas abaixo) |
| `EventType` | `string` | Categoria do evento: `"ActionExecution"` ou `"StateExecution"` |
| `FlowVersion` | `int?` | Versão do flow em execução |
| `Channel` | `string` | Domínio do canal de origem (extraído de `message.From.Domain`) |
| `IdMessage` | `string` | ID da mensagem que disparou o processamento |
| `From` | `string` | Identidade do usuário (`userIdentity`) |
| `To` | `string` | Identidade do owner (`ownerIdentity`) |
| `OriginalFrom` | `string` | Valor original de `message.From` |
| `OriginalTo` | `string` | Valor original de `message.To` |
| `Data` | `JObject` | Campos específicos do evento (ver cada seção) |

O método chamado no `IBlipLogger` é indicado em cada evento como **Logger method**.

---

## Eventos de ciclo de vida do Flow (FlowManager)

### InputProcessing

Disparado ao final do processamento de cada mensagem de entrada, independentemente de sucesso ou erro.

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `stateId` | `string` | ID do estado final após o processamento |
| `input` | `string` | Conteúdo serializado da mensagem de entrada |
| `inputExecutionTime` | `long` | Tempo total de execução em milissegundos |
| `error` | `string?` | Mensagem de erro, se houver |
| `inputTrace` | `object?` | Objeto `InputTrace` completo com rastreamento detalhado |
| `traceSettings` | `object?` | Configurações de trace ativas (`TraceSettings`) |

**Logger method:** `ActionExecution`  
**EventType:** `StateExecution`  
**Paths:** sucesso e erro (always fired no `finally`)

---

### OutputProcessing

Disparado após cada avaliação de saída de um estado (transição de estado).

#### Sucesso

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `currentStateId` | `string` | ID do estado atual (antes da transição) |
| `nextStateId` | `string?` | ID do próximo estado (null se não houver saída) |
| `outputsCount` | `int?` | Total de saídas configuradas no estado |
| `matchedOutputOrder` | `int?` | Ordem da saída que foi selecionada |
| `isDefaultOutput` | `bool?` | Indica se a saída selecionada é a padrão (sem condições) |
| `success` | `bool` | `true` |

**Logger method:** `ActionExecution`  
**EventType:** `StateExecution`

#### Erro

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `currentStateId` | `string` | ID do estado atual |
| `outputStateId` | `string` | ID do estado de destino da saída que falhou |
| `outputOrder` | `int` | Ordem da saída que falhou |
| `isDefaultOutput` | `bool` | Indica se era a saída padrão |
| `success` | `bool` | `false` |
| `error` | `string` | Stack trace da exceção |

**Logger method:** `ConversationalFlow`  
**EventType:** `StateExecution`

---

### SubflowEntry

Disparado quando o flow redireciona a execução para um subflow.

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do subflow sendo iniciado |
| `parentFlowId` | `string` | ID do flow pai que originou o redirecionamento |
| `currentStateId` | `string` | ID do estado no flow pai que acionou o subflow |
| `success` | `bool` | `true` |

**Logger method:** `ActionExecution`  
**EventType:** `StateExecution`  
**Path:** sucesso

---

### SubflowReturn

Disparado quando a execução retorna de um subflow para o flow pai.

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow pai para o qual está retornando |
| `subflowId` | `string` | ID do subflow que terminou |
| `nextStateId` | `string?` | ID do próximo estado no flow pai |
| `success` | `bool` | `true` |

**Logger method:** `ActionExecution`  
**EventType:** `StateExecution`  
**Path:** sucesso

---

### InputValidation

Disparado quando um estado tem validação configurada e a entrada **não** passa na validação. Não é disparado quando não há validação ou quando a entrada é válida.

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `stateId` | `string` | ID do estado cuja validação falhou |
| `validationRule` | `string` | Regra aplicada: `Text`, `Number`, `Date`, `Regex`, `Type` |
| `isValid` | `bool` | `false` (evento só é gerado quando a validação falha) |
| `success` | `bool` | `true` (a operação de validação em si foi bem-sucedida) |

**Logger method:** `ActionExecution`  
**EventType:** `StateExecution`  
**Path:** validação com falha

---

### MaxTransitionsReached

Disparado imediatamente antes de lançar `FlowConstructionException` quando o limite de transições de estado por mensagem é atingido (loop detectado no flow).

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `currentStateId` | `string?` | ID do estado no momento em que o limite foi atingido |
| `transitionCount` | `int` | Número de transições realizadas |
| `maxTransitions` | `int` | Limite configurado (`MaxTransitionsByInput`) |
| `success` | `bool` | `false` |

**Logger method:** `ActionExecution`  
**EventType:** `StateExecution`  
**Path:** erro

---

### CommandInput

Disparado no processamento de uma Local Custom Action (Builder Agent Command).

#### Sucesso

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `stateId` | `string` | ID do estado onde a ação customizada está configurada |
| `actionId` | `string` | ID da ação customizada executada |
| `success` | `bool` | `true` |

#### Erro

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `stateId` | `string` | ID do estado onde a ação customizada está configurada |
| `actionId` | `string` | ID da ação customizada executada |
| `success` | `bool` | `false` |
| `error` | `string` | Stack trace da exceção |

**Logger method:** `ActionExecution`  
**EventType:** `StateExecution`

---

## Eventos de Actions

Todos os eventos de action seguem o padrão abaixo, salvo campos específicos indicados em cada seção:

- **Logger method:** `ActionExecution`
- **EventType:** `"ActionExecution"`
- **Campos sempre presentes:** `flowId`, `success` (+ `error` no path de erro)

---

### CreateTicket

Ação que cria um ticket de atendimento humano.

#### Sucesso

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `customerIdentity` | `string` | Identidade do cliente para o qual o ticket foi criado |
| `outputVariable` | `string` | Nome da variável de contexto onde o resultado é armazenado |
| `ticketId` | `string?` | ID do ticket criado |
| `success` | `bool` | `true` |

#### Erro

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `customerIdentity` | `string` | Identidade do cliente (das configurações da ação) |
| `outputVariable` | `string` | Nome da variável de contexto configurada |
| `success` | `bool` | `false` |
| `error` | `string` | Stack trace da exceção |

---

### DeleteVariable

Ação que remove uma variável de contexto do usuário.

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `variable` | `string` | Nome da variável removida |
| `success` | `bool` | `true` / `false` |
| `error` | `string?` | Stack trace (apenas no path de erro) |

---

### ExecuteScript

Ação que executa um script JavaScript (engine Jint — versão 1).

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `function` | `string` | Nome da função executada (padrão: `run`) |
| `outputVariable` | `string` | Variável de contexto onde o retorno é armazenado |
| `success` | `bool` | `true` / `false` |
| `error` | `string?` | Stack trace (apenas no path de erro) |

---

### ExecuteScriptV2

Ação que executa um script JavaScript (engine Jint — versão 2, com suporte a captureExceptions).

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `function` | `string` | Nome da função executada |
| `outputVariable` | `string` | Variável de contexto onde o retorno é armazenado |
| `captureExceptions` | `bool` | Se exceções internas do script são capturadas como retorno |
| `success` | `bool` | `true` / `false` |
| `error` | `string?` | Stack trace (apenas no path de erro) |

---

### ExecuteTemplate

Ação que renderiza um template Handlebars e armazena o resultado.

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `outputVariable` | `string` | Variável de contexto onde o resultado renderizado é armazenado |
| `success` | `bool` | `true` / `false` |
| `error` | `string?` | Stack trace (apenas no path de erro) |

---

### ManageList

Ação que adiciona ou remove um contato de uma lista de distribuição.

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `listName` | `string` | Nome da lista de distribuição |
| `listAction` | `string` | Operação realizada: `Add` ou `Remove` |
| `success` | `bool` | `true` / `false` |
| `error` | `string?` | Stack trace (apenas no path de erro) |

---

### MergeContact

Ação que mescla dados de contato no perfil do usuário.

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `success` | `bool` | `true` / `false` |
| `error` | `string?` | Stack trace (apenas no path de erro) |

---

### ProcessCommand

Ação que envia um comando LIME e armazena a resposta.

#### Sucesso

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `uri` | `string` | URI do comando enviado |
| `method` | `string` | Método do comando: `Get`, `Set`, `Delete`, etc. |
| `outputVariable` | `string` | Variável de contexto onde a resposta é armazenada |
| `success` | `bool` | `true` |

#### Erro

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `success` | `bool` | `false` |
| `error` | `string` | Stack trace da exceção |

---

### ProcessContentAssistant

Ação que consulta o Content Assistant (AI para classificação de conteúdo).

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `outputVariable` | `string` | Variável onde o resultado da classificação é armazenado |
| `v2` | `bool` | Indica se está usando a versão 2 da API do Content Assistant |
| `success` | `bool` | `true` / `false` |
| `error` | `string?` | Stack trace (apenas no path de erro) |

---

### ProcessHttp

Ação que realiza uma requisição HTTP externa.

#### Sucesso / HTTP Error (response recebida)

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `uri` | `string` | URL requisitada |
| `method` | `string` | Método HTTP: `GET`, `POST`, `PUT`, `DELETE`, etc. |
| `responseStatus` | `string?` | Código de status HTTP retornado (ex.: `"200"`, `"404"`) |
| `success` | `bool` | `true` se status 2xx, `false` caso contrário |

#### Erro de rede (HttpRequestException)

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `uri` | `string` | URL requisitada |
| `method` | `string` | Método HTTP |
| `responseStatus` | `string?` | Status HTTP, quando disponível |
| `success` | `bool` | `false` |
| `error` | `string` | Stack trace da exceção |

#### Erro geral (Exception)

Mesmos campos do path de erro de rede acima.

---

### Redirect

Ação que redireciona o usuário para outro bot ou fluxo externo.

#### Sucesso

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `address` | `string` | Endereço de destino do redirecionamento |
| `success` | `bool` | `true` |

#### Erro

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `success` | `bool` | `false` |
| `error` | `string` | Stack trace da exceção |

---

### SendCommand

Ação que envia um comando LIME sem aguardar resposta.

#### Sucesso

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `uri` | `string` | URI do comando |
| `method` | `string` | Método do comando |
| `success` | `bool` | `true` |

#### Erro

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `success` | `bool` | `false` |
| `error` | `string` | Stack trace da exceção |

---

### SendMessage

Ação que envia uma mensagem ao usuário.

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `contentType` | `string` | Media type do conteúdo (ex.: `text/plain`, `application/vnd.lime.select+json`) |
| `success` | `bool` | `true` / `false` |
| `error` | `string?` | Stack trace (apenas no path de erro) |

---

### SendMessageFromHttp

Ação que busca o conteúdo de uma URL e envia como mensagem ao usuário.

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `uri` | `string` | URL de onde o conteúdo da mensagem é obtido |
| `responseStatus` | `string?` | Código de status HTTP da requisição de conteúdo |
| `success` | `bool` | `true` / `false` |
| `error` | `string?` | Stack trace (apenas no path de erro) |

---

### SendRawMessage

Ação que envia uma mensagem com conteúdo bruto (raw).

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `mediaType` | `string` | Media type do conteúdo bruto enviado |
| `success` | `bool` | `true` / `false` |
| `error` | `string?` | Stack trace (apenas no path de erro) |

---

### SetBucket

Ação que armazena um documento no Bucket (armazenamento persistente por chave).

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `bucketId` | `string` | Chave do documento armazenado |
| `expiration` | `int?` | TTL em minutos (null = sem expiração) |
| `success` | `bool` | `true` / `false` |
| `error` | `string?` | Stack trace (apenas no path de erro) |

---

### SetVariable

Ação que define o valor de uma variável de contexto do usuário.

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `variable` | `string` | Nome da variável definida |
| `expiration` | `int?` | TTL em minutos (null = sem expiração) |
| `success` | `bool` | `true` / `false` |
| `error` | `string?` | Stack trace (apenas no path de erro) |

---

### TrackContactsJourney

Ação que registra a jornada do contato no painel de análise.

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `stateId` | `string` | ID do estado atual registrado na jornada |
| `stateName` | `string` | Nome legível do estado atual |
| `previousStateId` | `string` | ID do estado anterior na jornada |
| `success` | `bool` | `true` / `false` |
| `error` | `string?` | Stack trace (apenas no path de erro) |

---

### TrackEvent

Ação que registra um evento analítico (categoria/ação/label).

| Campo | Tipo | Descrição |
|---|---|---|
| `flowId` | `string` | ID do flow |
| `category` | `string` | Categoria do evento analítico |
| `action` | `string` | Ação do evento analítico |
| `label` | `string` | Label do evento analítico |
| `success` | `bool` | `true` / `false` |
| `error` | `string?` | Stack trace (apenas no path de erro) |

---

## Resumo de todos os eventos

| Evento | EventType | Logger method | Paths | Fonte |
|---|---|---|---|---|
| `InputProcessing` | `StateExecution` | `ActionExecution` | sempre (finally) | FlowManager |
| `OutputProcessing` (sucesso) | `StateExecution` | `ActionExecution` | sucesso | FlowManager |
| `OutputProcessing` (erro) | `StateExecution` | `ConversationalFlow` | erro | FlowManager |
| `SubflowEntry` | `StateExecution` | `ActionExecution` | sucesso | FlowManager |
| `SubflowReturn` | `StateExecution` | `ActionExecution` | sucesso | FlowManager |
| `InputValidation` | `StateExecution` | `ActionExecution` | falha de validação | FlowManager |
| `MaxTransitionsReached` | `StateExecution` | `ActionExecution` | erro | FlowManager |
| `CommandInput` | `StateExecution` | `ActionExecution` | sucesso + erro | FlowManager |
| `CreateTicket` | `ActionExecution` | `ActionExecution` | sucesso + erro | Action |
| `DeleteVariable` | `ActionExecution` | `ActionExecution` | sucesso + erro | Action |
| `ExecuteScript` | `ActionExecution` | `ActionExecution` | sucesso + erro | Action |
| `ExecuteScriptV2` | `ActionExecution` | `ActionExecution` | sucesso + erro | Action |
| `ExecuteTemplate` | `ActionExecution` | `ActionExecution` | sucesso + erro | Action |
| `ManageList` | `ActionExecution` | `ActionExecution` | sucesso + erro | Action |
| `MergeContact` | `ActionExecution` | `ActionExecution` | sucesso + erro | Action |
| `ProcessCommand` | `ActionExecution` | `ActionExecution` | sucesso + erro | Action |
| `ProcessContentAssistant` | `ActionExecution` | `ActionExecution` | sucesso + erro | Action |
| `ProcessHttp` | `ActionExecution` | `ActionExecution` | sucesso + 2× erro | Action |
| `Redirect` | `ActionExecution` | `ActionExecution` | sucesso + erro | Action |
| `SendCommand` | `ActionExecution` | `ActionExecution` | sucesso + erro | Action |
| `SendMessage` | `ActionExecution` | `ActionExecution` | sucesso + erro | Action |
| `SendMessageFromHttp` | `ActionExecution` | `ActionExecution` | sucesso + erro | Action |
| `SendRawMessage` | `ActionExecution` | `ActionExecution` | sucesso + erro | Action |
| `SetBucket` | `ActionExecution` | `ActionExecution` | sucesso + erro | Action |
| `SetVariable` | `ActionExecution` | `ActionExecution` | sucesso + erro | Action |
| `TrackContactsJourney` | `ActionExecution` | `ActionExecution` | sucesso + erro | Action |
| `TrackEvent` | `ActionExecution` | `ActionExecution` | sucesso + erro | Action |

**Total de eventos distintos:** 27  
**Total de chamadas ao IBlipLogger:** 48
