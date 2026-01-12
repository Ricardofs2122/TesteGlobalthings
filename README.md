## TesteGlobalthings
Parte 01: 
a) Resposta: Criaria uma API com um endpoint para recebimento de medições em lote (batch),  "POST /medicoes" e uma chave unica entre ID e DataHoraMedicao para evitar duplicidade no banco.  

Justificativa:
Envio em Lote: Como o firmware pode acumular dados quando o WiFi está intermitente, a API precisa aceitar várias medições no mesmo request

Chave Única: Caso os sensores dem algum problema ou reenviem dados duplicados é possivel garantir que os dados não foram duplicados.
