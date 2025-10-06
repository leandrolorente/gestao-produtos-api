# 📋 **API de Fornecedores - Documentação para Frontend**

## 🔗 **Base URL:** `http://localhost:5278/api/fornecedores`

### 🔐 **Autenticação**
- **Todas as rotas requerem autenticação JWT**
- Header: `Authorization: Bearer {token}`

---

## 📊 **Endpoints Disponíveis**

### **1. 📋 GET /api/fornecedores**
**Descrição:** Obtém todos os fornecedores  
**Cache:** ✅ Redis (5 min TTL)  
**Response:** `Array<FornecedorDto>`

```typescript
interface FornecedorDto {
  id: string;
  razaoSocial: string;
  nomeFantasia?: string;
  cnpjCpf: string;
  email: string;
  telefone: string;
  endereco?: EnderecoDto;
  inscricaoEstadual?: string;
  inscricaoMunicipal?: string;
  tipo: string; // "Nacional" | "Internacional"
  status: string; // "Ativo" | "Inativo" | "Bloqueado"
  observacoes?: string;
  contatoPrincipal?: string;
  site?: string;
  
  // Dados bancários
  banco?: string;
  agencia?: string;
  conta?: string;
  pix?: string;
  
  // Condições comerciais
  prazoPagamentoPadrao: number;
  limiteCredito: number;
  condicoesPagamento?: string;
  
  // Estatísticas
  quantidadeProdutos: number;
  ultimaCompra?: string; // ISO date
  totalComprado: number;
  quantidadeCompras: number;
  ticketMedio: number;
  ehFrequente: boolean;
  
  // Auditoria
  dataCriacao: string; // ISO date
  dataAtualizacao: string; // ISO date
  ativo: boolean;
}
```

---

### **2. 📋 GET /api/fornecedores/list**
**Descrição:** Obtém lista simplificada de fornecedores  
**Cache:** ✅ Redis (5 min TTL)  
**Response:** `Array<FornecedorListDto>`

```typescript
interface FornecedorListDto {
  id: string;
  razaoSocial: string;
  nomeFantasia?: string;
  cnpjCpf: string;
  email: string;
  telefone: string;
  tipo: string;
  status: string;
  contatoPrincipal?: string;
  ultimaCompra?: string;
  totalComprado: number;
  quantidadeCompras: number;
  ehFrequente: boolean;
  ativo: boolean;
}
```

---

### **3. 🔍 GET /api/fornecedores/{id}**
**Descrição:** Obtém um fornecedor por ID  
**Cache:** ✅ Redis (5 min TTL)  
**Response:** `FornecedorDto`

---

### **4. 🔍 GET /api/fornecedores/cnpj/{cnpjCpf}**
**Descrição:** Obtém fornecedor por CNPJ/CPF  
**Response:** `FornecedorDto`

---

### **5. 🔍 GET /api/fornecedores/buscar?termo={termo}**
**Descrição:** Busca fornecedores por termo (nome, cnpj, email)  
**Response:** `Array<FornecedorDto>`

---

### **6. 🔍 GET /api/fornecedores/tipo/{tipo}**
**Descrição:** Obtém fornecedores por tipo  
**Parâmetros:** `tipo: "Nacional" | "Internacional"`  
**Response:** `Array<FornecedorDto>`

---

### **7. 🔍 GET /api/fornecedores/status/{status}**
**Descrição:** Obtém fornecedores por status  
**Parâmetros:** `status: "Ativo" | "Inativo" | "Bloqueado"`  
**Response:** `Array<FornecedorDto>`

---

### **8. 🔍 GET /api/fornecedores/compra-recente?dias={dias}**
**Descrição:** Obtém fornecedores com compra recente  
**Parâmetros:** `dias: number` (default: 90)  
**Response:** `Array<FornecedorDto>`

---

### **9. 🔍 GET /api/fornecedores/frequentes**
**Descrição:** Obtém fornecedores frequentes (5+ compras)  
**Response:** `Array<FornecedorDto>`

---

### **10. 🔍 GET /api/fornecedores/produto/{produtoId}**
**Descrição:** Obtém fornecedores de um produto específico  
**Response:** `Array<FornecedorDto>`

---

### **11. ➕ POST /api/fornecedores**
**Descrição:** Cria um novo fornecedor  
**Body:** `CreateFornecedorDto`  
**Response:** `FornecedorDto`

```typescript
interface CreateFornecedorDto {
  razaoSocial: string;
  nomeFantasia?: string;
  cnpjCpf: string;
  email: string;
  telefone: string;
  endereco?: CreateEnderecoDto;
  inscricaoEstadual?: string;
  inscricaoMunicipal?: string;
  tipo: "Nacional" | "Internacional";
  observacoes?: string;
  contatoPrincipal?: string;
  site?: string;
  
  // Dados bancários
  banco?: string;
  agencia?: string;
  conta?: string;
  pix?: string;
  
  // Condições comerciais
  prazoPagamentoPadrao: number;
  limiteCredito: number;
  condicoesPagamento?: string;
}
```

---

### **12. ✏️ PUT /api/fornecedores/{id}**
**Descrição:** Atualiza um fornecedor  
**Body:** `UpdateFornecedorDto`  
**Response:** `FornecedorDto`

```typescript
interface UpdateFornecedorDto {
  razaoSocial: string;
  nomeFantasia?: string;
  email: string;
  telefone: string;
  endereco?: UpdateEnderecoDto;
  inscricaoEstadual?: string;
  inscricaoMunicipal?: string;
  tipo: "Nacional" | "Internacional";
  status: "Ativo" | "Inativo" | "Bloqueado";
  observacoes?: string;
  contatoPrincipal?: string;
  site?: string;
  
  // Dados bancários
  banco?: string;
  agencia?: string;
  conta?: string;
  pix?: string;
  
  // Condições comerciais
  prazoPagamentoPadrao: number;
  limiteCredito: number;
  condicoesPagamento?: string;
}
```

---

### **13. 🗑️ DELETE /api/fornecedores/{id}**
**Descrição:** Remove um fornecedor (soft delete)  
**Response:** `204 No Content`

---

### **14. 🚫 POST /api/fornecedores/{id}/bloquear**
**Descrição:** Bloqueia um fornecedor  
**Body:** `{ motivo?: string }`  
**Response:** `200 OK`

---

### **15. ✅ POST /api/fornecedores/{id}/desbloquear**
**Descrição:** Desbloqueia um fornecedor  
**Response:** `200 OK`

---

### **16. ⏸️ POST /api/fornecedores/{id}/inativar**
**Descrição:** Inativa um fornecedor  
**Response:** `200 OK`

---

### **17. ▶️ POST /api/fornecedores/{id}/ativar**
**Descrição:** Ativa um fornecedor  
**Response:** `200 OK`

---

### **18. 📦 POST /api/fornecedores/{fornecedorId}/produtos/{produtoId}**
**Descrição:** Adiciona produto ao fornecedor  
**Response:** `200 OK`

---

### **19. 📦 DELETE /api/fornecedores/{fornecedorId}/produtos/{produtoId}**
**Descrição:** Remove produto do fornecedor  
**Response:** `200 OK`

---

### **20. 💰 POST /api/fornecedores/{fornecedorId}/compras**
**Descrição:** Registra uma compra  
**Body:** `{ valor: number }`  
**Response:** `200 OK`

---

### **21. 💼 PUT /api/fornecedores/{fornecedorId}/condicoes-comerciais**
**Descrição:** Atualiza condições comerciais  
**Body:**
```typescript
{
  prazoPagamento: number;
  limiteCredito: number;
  condicoesPagamento?: string;
}
```
**Response:** `200 OK`

---

## 🏗️ **DTOs de Endereço**

```typescript
interface EnderecoDto {
  id: string;
  cep: string;
  logradouro: string;
  numero: string;
  complemento?: string;
  unidade: string;
  bairro: string;
  localidade: string;
  uf: string;
  estado: string;
  regiao: string;
  referencia?: string;
  isPrincipal: boolean;
  tipo: string;
  ativo: boolean;
  dataCriacao: string;
  dataAtualizacao: string;
}

interface CreateEnderecoDto {
  cep: string;
  logradouro: string;
  numero: string;
  complemento?: string;
  unidade: string;
  bairro: string;
  localidade: string;
  uf: string;
  estado: string;
  regiao: string;
  referencia?: string;
  isPrincipal: boolean;
  tipo: string;
}
```

---

## 📝 **Códigos de Resposta**

- **200 OK** - Sucesso
- **201 Created** - Recurso criado
- **204 No Content** - Sucesso sem conteúdo
- **400 Bad Request** - Dados inválidos
- **401 Unauthorized** - Token inválido/ausente
- **404 Not Found** - Recurso não encontrado
- **409 Conflict** - CNPJ já existe
- **500 Internal Server Error** - Erro interno

---

## 🚀 **Performance & Cache**

- **Cache Redis:** 5 minutos TTL para consultas
- **Logs visuais:** Console logs para debug (🚀 Cache Hit, 🗄️ Database)
- **Invalidação:** Automática em operações CUD

---

## 📍 **Validações**

- **CNPJ/CPF:** Validação automática
- **Email:** Formato válido obrigatório
- **Telefone:** Formato brasileiro
- **Endereço:** CEP com integração ViaCEP

---

## 🔧 **Exemplo de Uso (Angular)**

```typescript
// Service
@Injectable()
export class FornecedorService {
  private apiUrl = 'http://localhost:5278/api/fornecedores';

  constructor(private http: HttpClient) {}

  getAll(): Observable<FornecedorDto[]> {
    return this.http.get<FornecedorDto[]>(this.apiUrl);
  }

  getById(id: string): Observable<FornecedorDto> {
    return this.http.get<FornecedorDto>(`${this.apiUrl}/${id}`);
  }

  create(fornecedor: CreateFornecedorDto): Observable<FornecedorDto> {
    return this.http.post<FornecedorDto>(this.apiUrl, fornecedor);
  }

  update(id: string, fornecedor: UpdateFornecedorDto): Observable<FornecedorDto> {
    return this.http.put<FornecedorDto>(`${this.apiUrl}/${id}`, fornecedor);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  bloquear(id: string, motivo?: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/bloquear`, { motivo });
  }

  desbloquear(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/desbloquear`, {});
  }

  buscar(termo: string): Observable<FornecedorDto[]> {
    return this.http.get<FornecedorDto[]>(`${this.apiUrl}/buscar?termo=${termo}`);
  }

  registrarCompra(fornecedorId: string, valor: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${fornecedorId}/compras`, { valor });
  }
}
```

---

## 📱 **Exemplo de Interface Angular**

```typescript
// Component
export interface FornecedorFormData {
  razaoSocial: string;
  nomeFantasia?: string;
  cnpjCpf: string;
  email: string;
  telefone: string;
  tipo: 'Nacional' | 'Internacional';
  endereco?: {
    cep: string;
    logradouro: string;
    numero: string;
    bairro: string;
    cidade: string;
    uf: string;
  };
  dadosBancarios?: {
    banco?: string;
    agencia?: string;
    conta?: string;
    pix?: string;
  };
  condicoesComerciais?: {
    prazoPagamento: number;
    limiteCredito: number;
    condicoesPagamento?: string;
  };
}
```

---

## 🎯 **Dicas de Implementação**

1. **Cache no Frontend:** Considere implementar cache local para dados frequentes
2. **Paginação:** Para listas grandes, implemente paginação client-side
3. **Validação:** Valide CNPJ/CPF no frontend antes de enviar
4. **Loading States:** Use skeletons durante carregamento
5. **Error Handling:** Trate erros 409 (CNPJ duplicado) adequadamente
6. **Real-time:** Considere WebSockets para updates em tempo real

---

## 🚨 **Importantes**

- Todos os campos de data estão em formato ISO 8601
- Valores monetários são em decimal (precisão)
- IDs são strings (MongoDB ObjectId)
- Cache é invalidado automaticamente em operações CUD
- Logs visuais aparecem no console da API para debug

---

**Versão:** 1.0.0  
**Última atualização:** 5 de outubro de 2025  
**Status:** ✅ Produção Ready