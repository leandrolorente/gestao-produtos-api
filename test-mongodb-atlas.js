// Script para testar conexão MongoDB Atlas
// Execute com: node test-mongodb-atlas.js

const { MongoClient } = require('mongodb');

// Sua connection string (substitua <db_username> e <db_password>)
const uri = "mongodb+srv://<db_username>:<db_password>@cluster0.y60kygq.mongodb.net/GestaoProdutosDB?retryWrites=true&w=majority&appName=Cluster0";

async function testConnection() {
    const client = new MongoClient(uri);
    
    try {
        console.log('🔄 Conectando ao MongoDB Atlas...');
        await client.connect();
        
        console.log('✅ Conectado com sucesso!');
        
        // Testar acesso ao banco
        const db = client.db('GestaoProdutosDB');
        
        // Listar coleções
        const collections = await db.listCollections().toArray();
        console.log('📋 Coleções encontradas:', collections.map(c => c.name));
        
        // Testar inserção simples
        const testCollection = db.collection('test');
        const result = await testCollection.insertOne({ 
            test: true, 
            timestamp: new Date(),
            message: 'Teste de conexão Atlas' 
        });
        
        console.log('✅ Teste de inserção:', result.insertedId);
        
        // Limpar teste
        await testCollection.deleteOne({ _id: result.insertedId });
        console.log('🧹 Limpeza concluída');
        
    } catch (error) {
        console.error('❌ Erro de conexão:', error.message);
        
        if (error.message.includes('authentication')) {
            console.log('💡 Verifique usuário/senha no Atlas');
        }
        if (error.message.includes('network')) {
            console.log('💡 Verifique IP whitelist (0.0.0.0/0)');
        }
        if (error.message.includes('ENOTFOUND')) {
            console.log('💡 Verifique a URL do cluster');
        }
    } finally {
        await client.close();
        console.log('🔒 Conexão fechada');
    }
}

testConnection();