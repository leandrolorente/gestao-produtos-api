// Script de inicialização do MongoDB
// Este script é executado quando o container do MongoDB é criado pela primeira vez

print('Iniciando configuração do banco de dados GestaoProdutosDB...');

// Criar banco de dados e usuário específico
db = db.getSiblingDB('GestaoProdutosDB');

// Criar usuário específico para a aplicação
db.createUser({
  user: 'gestao_user',
  pwd: 'gestao_password_123',
  roles: [
    {
      role: 'readWrite',
      db: 'GestaoProdutosDB'
    }
  ]
});

// Criar coleções básicas
db.createCollection('produtos');
db.createCollection('clientes');
db.createCollection('usuarios');
db.createCollection('vendas');

// Criar índices básicos
db.produtos.createIndex({ "Sku": 1 }, { unique: true });
db.produtos.createIndex({ "Nome": 1 });
db.produtos.createIndex({ "Categoria": 1 });
db.produtos.createIndex({ "Barcode": 1 }, { unique: true, sparse: true });

db.clientes.createIndex({ "CpfCnpj.Valor": 1 }, { unique: true });
db.clientes.createIndex({ "Email.Valor": 1 }, { unique: true });
db.clientes.createIndex({ "Nome": 1 });

db.usuarios.createIndex({ "Email.Valor": 1 }, { unique: true });
db.usuarios.createIndex({ "Nome": 1 });

db.vendas.createIndex({ "Numero": 1 }, { unique: true });
db.vendas.createIndex({ "ClienteId": 1 });
db.vendas.createIndex({ "VendedorId": 1 });
db.vendas.createIndex({ "Status": 1 });
db.vendas.createIndex({ "DataVenda": 1 });
db.vendas.createIndex({ "DataVencimento": 1 });

// Inserir usuário administrador padrão
db.usuarios.insertOne({
  "_id": ObjectId(),
  "Nome": "Administrador",
  "Email": {
    "Valor": "admin@gestao.com"
  },
  "Role": 1, // Admin
  "Avatar": "https://ui-avatars.com/api/?name=Admin&background=007bff&color=fff",
  "Departamento": "TI",
  "SenhaHash": "ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f", // senha: admin123 com salt JWT Secret
  "Ativo": true,
  "DataCriacao": new Date(),
  "DataAtualizacao": new Date(),
  "UltimoLogin": null
});

print('Configuração do banco de dados concluída!');
print('Usuário admin criado: admin@gestao.com / admin123');
print('Base URL da API: http://localhost:5000');
print('MongoDB Express: http://localhost:8081 (admin/pass)');