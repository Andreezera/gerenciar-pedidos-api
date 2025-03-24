# Gerenciar Pedidos API

API para gerenciamento de pedidos

## Tecnologias Utilizadas
- **.NET 8**
- **Arquitetura em Camadas (API, Domain, Data)**
- **Injeção de Dependências (Dependency Injection)**
- **Cache com IMemoryCache**
- **Testes Unitários (xUnit, FluentAssertions)**
- **Testes de Integração com WebApplicationFactory**
- **Swagger para documentação da API**
- **Serilog para logging**

---

## Como Executar o Projeto

### **1 - Clonar o repositório**
```bash
git clone https://github.com/seu-usuario/gerenciar-pedidos-api.git
cd gerenciar-pedidos-api
```

### **2 - Configurar e rodar a API**
1. Abra o projeto no **Visual Studio**
2. Selecione `GerenciarPedidos.API` como **projeto de inicialização**
3. Pressione `F5` para rodar

---

## 📝 **Documentação da API (Swagger)**
Após iniciar a API, acesse o Swagger:
**[http://localhost:5000/index.html](http://localhost:5000/index.html)**

---

## 🔧 **Configuração do Cálculo de Imposto**
A API permite alternar entre diferentes regras de cálculo de imposto utilizando uma **Feature Flag** definida no `appsettings.json`. 

Exemplo:
```json
{
  "FeatureFlags": {
    "UsarReformaTributaria": true
  }
}
```

- Se `UsarReformaTributaria` estiver como `true`, será usado o **novo cálculo**: `Valor Total dos Itens * 0.2`
- Se `false`, o sistema usa o **cálculo antigo**: `Valor Total dos Itens * 0.3`

Isso permite ativar o novo modelo sem necessidade de alterar o código.

---

## 🔥 **Principais Decisões Técnicas**
✅ **Uso de Cache:** Implementado com `IMemoryCache` para otimizar listagem de pedidos.  
✅ **Feature Flags:** Implementação do cálculo de imposto com opção para troca de regras tributárias.  
✅ **Logging:** Uso de **Serilog** para logs estruturados e monitoramento detalhado.  
✅ **Testes Automatizados:** Cobertura completa com testes unitários e de integração.  
✅ **Padrões SOLID e Clean Code:** Código modular e organizado, seguindo boas práticas de desenvolvimento.
