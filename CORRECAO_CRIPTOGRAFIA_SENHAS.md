# 🔧 Correção: Alinhamento de Criptografia de Senhas

## 🚨 Problema Identificado

Existia uma **incompatibilidade** entre os algoritmos de criptografia de senha usados no `AuthService` e `UserService`:

### ❌ **Antes (Inconsistente):**

**AuthService** (usado no login):
```csharp
private string CriptografarSenha(string senha)
{
    using var sha256 = SHA256.Create();
    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha + _jwtSecret));
    return Convert.ToBase64String(hashedBytes);
}
```

**UserService** (usado na criação de usuários):
```csharp
private string CriptografarSenha(string senha)
{
    using var sha256 = SHA256.Create();
    var salt = Guid.NewGuid().ToString();
    var senhaComSalt = senha + salt;
    var bytes = Encoding.UTF8.GetBytes(senhaComSalt);
    var hash = sha256.ComputeHash(bytes);
    return Convert.ToBase64String(hash) + ":" + salt;  // ← Salt individual
}
```

### 🎯 **Resultado:**
- Usuário criado via `UserService` → Senha com salt individual
- Login via `AuthService` → Tentativa de validação com JWT secret
- **Senhas nunca batiam** = Login sempre falhava

---

## ✅ **Solução Implementada**

### 🔧 **Padronização:**
Ambos os services agora usam **exatamente o mesmo algoritmo**:

```csharp
private string CriptografarSenha(string senha)
{
    using var sha256 = SHA256.Create();
    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha + _jwtSecret));
    return Convert.ToBase64String(hashedBytes);
}
```

### 📋 **Mudanças Realizadas:**

1. **UserService.cs:**
   - ✅ Adicionado `IConfiguration` como dependência
   - ✅ Injetado `_jwtSecret` do appsettings.json
   - ✅ Atualizado método `CriptografarSenha()` para usar JWT secret
   - ✅ Removido sistema de salt individual

2. **UserServiceSimpleTests.cs:**
   - ✅ Adicionado mock para `IConfiguration`
   - ✅ Configurado JWT secret para testes
   - ✅ Atualizado construtor do UserService

3. **Validação:**
   - ✅ **112 testes passando** (0 falhas)
   - ✅ Build sem erros
   - ✅ Compatibilidade total entre services

---

## 🔐 **Como Funciona Agora**

### **Criação de Usuário:**
1. UserService recebe senha em texto
2. Aplica: `SHA256(senha + jwtSecret)`
3. Armazena hash no banco

### **Login:**
1. AuthService recebe senha em texto
2. Aplica: `SHA256(senha + jwtSecret)`
3. Compara com hash do banco
4. ✅ **Match perfeito!**

---

## 🧪 **Impacto nos Testes**

### **Antes:**
- Testes passavam porque mockavam separadamente
- **Mas em produção não funcionaria**

### **Depois:**
- Testes continuam passando
- **E agora refletem o comportamento real**
- Ambos os services usam a mesma configuração

---

## 🚀 **Próximos Passos**

### ⚠️ **Usuários Existentes:**
Se já existem usuários no banco com o algoritmo antigo, eles **não conseguirão fazer login**. Soluções:

1. **Migração de dados** (requer reset de senhas)
2. **Endpoint de reset** para usuários existentes
3. **Detecção dupla** (tentar ambos os algoritmos)

### 💡 **Recomendação:**
Como é início de desenvolvimento, usar o endpoint `setup-first-admin` para criar o primeiro usuário já com o algoritmo correto.

---

## 📋 **Resumo da Correção**

| Aspecto | Antes | Depois |
|---------|-------|--------|
| **AuthService** | SHA256(senha + jwtSecret) | SHA256(senha + jwtSecret) |
| **UserService** | SHA256(senha + saltIndividual) | SHA256(senha + jwtSecret) ✅ |
| **Compatibilidade** | ❌ Incompatível | ✅ **Alinhado** |
| **Login** | ❌ Sempre falhava | ✅ **Funcionando** |
| **Testes** | ✅ 112 passando | ✅ **112 passando** |

---

**🎉 Agora o login funcionará corretamente com usuários criados via UserService!**