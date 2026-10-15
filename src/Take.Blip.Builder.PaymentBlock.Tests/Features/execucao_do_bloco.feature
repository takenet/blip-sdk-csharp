# language: pt
@regressao @bloco-pagamento
Funcionalidade: Execução do bloco de Pagamentos no motor do Builder
  O fluxo gerado pelo bloco (referência aprovada "checkout-v1", exportada pela suíte do
  portal-submodule-builder) roda no mesmo motor usado pela Template Hosting, com os comandos
  e as chamadas HTTP simulados. Protege o v1 enquanto ele existir; o v2 (ação nativa Payment) está em acao_nativa_payment.feature.

  Contexto:
    Dado o fluxo de referência "checkout-v1" carregado no motor do Builder
    E o bucket "checkout-secret" do bot com um segredo de teste
    E a checkout-auth-api e a checkout-api simuladas respondendo normalmente

  Cenário: Cliente entra no bloco e recebe o pedido com o link
    Quando o cliente envia a mensagem que leva ao bloco de Pagamentos
    Então o motor lê o bucket "checkout-secret" por um comando marcado para passar pelo Iris Server
    E chama a checkout-auth-api para obter o token do bot
    E chama a checkout-api para criar o link de pagamento
    E envia ao cliente o cartão de pedido com o link

  Cenário: Falha da checkout-api não envia link
    Dado que a checkout-api simulada responde erro na criação do link
    Quando o cliente envia a mensagem que leva ao bloco de Pagamentos
    Então nenhum cartão de pedido é enviado ao cliente
    E a falha fica registrada pelo motor

  Cenário: Script do bloco respeita o tempo máximo de execução
    Dado que a checkout-api simulada demora mais que o tempo máximo de script configurado
    Quando o cliente envia a mensagem que leva ao bloco de Pagamentos
    Então a execução do script é interrompida por tempo
    E nenhum cartão de pedido é enviado ao cliente
