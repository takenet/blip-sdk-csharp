# language: pt
@regressao @bloco-pagamento @referencia-v1
Funcionalidade: Mensagens enviadas pelo bloco de Pagamentos v1 (referência para o backend)
  O bloco v1 roda no motor do Builder com a checkout-api e a checkout-auth-api simuladas.
  As mensagens que ele envia são gravadas como referência aprovada e usadas pela
  payment-application para provar que gera as mesmas mensagens quando a lógica sai do front.

  Esquema do Cenário: Mensagem do pedido para cada configuração do bloco
    Dado o fluxo de referência "checkout-v1" com a configuração "<configuracao>"
    E a checkout-api simulada respondendo normalmente
    Quando o cliente envia a mensagem que leva ao bloco de Pagamentos
    Então a mensagem enviada ao cliente é igual à referência "<referencia>", exceto campos voláteis

    Exemplos:
      | configuracao                    | referencia                  |
      | checkout-link                   | v1-checkout-link            |
      | checkout-pix                    | v1-checkout-pix             |
      | checkout-link-e-pix             | v1-checkout-link-e-pix      |
      | checkout-link-com-cartao-salvo  | v1-checkout-cta-cartao      |
      | proprio-pix-cpf                 | v1-proprio-pix-cpf          |
      | proprio-pix-aleatoria           | v1-proprio-pix-aleatoria    |
      | proprio-link                    | v1-proprio-link             |
      | proprio-boleto                  | v1-proprio-boleto           |
      | produto-fisico-dois-itens       | v1-fisico-dois-itens        |

  Esquema do Cenário: Mensagem de status para cada resultado do pagamento
    Dado o fluxo de referência "checkout-v1" com a configuração "checkout-link-e-pix" e feedback ligado
    E a checkout-api simulada informando o pagamento por "<meio>" com status "<status>"
    Quando o tempo de feedback termina
    Então a mensagem de status enviada é igual à referência "<referencia>", exceto campos voláteis

    Exemplos:
      | meio       | status     | referencia            |
      | PIX        | Succeeded  | v1-status-pix-pago    |
      | CreditCard | Succeeded  | v1-status-cartao-pago |
      | CreditCard | Denied     | v1-status-negado      |
      | PIX        | Pending    | v1-status-pendente    |
      | PIX        | Created    | v1-status-criado      |
