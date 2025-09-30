using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace TestMongoAtlas
{
    class Program
    {
        // Sua connection string com credenciais do MongoDB Atlas
        private static readonly string ConnectionString = 
            "mongodb+srv://gestaoProdutosBD:427931%40%21%40@cluster0.y60kygq.mongodb.net/GestaoProdutosDB?retryWrites=true&w=majority&appName=Cluster0";
        
        private static readonly string DatabaseName = "GestaoProdutosDB";

        static async Task Main(string[] args)
        {
            Console.WriteLine("🔄 Testando conexão MongoDB Atlas...");
            
            try
            {
                var client = new MongoClient(ConnectionString);
                var database = client.GetDatabase(DatabaseName);
                
                // Teste de conexão simples
                Console.WriteLine("✅ Cliente MongoDB criado");
                
                // Testar acesso ao banco
                var collections = await database.ListCollectionNamesAsync();
                var collectionList = await collections.ToListAsync();
                
                Console.WriteLine($"✅ Conectado ao banco: {DatabaseName}");
                Console.WriteLine($"📋 Coleções encontradas: {collectionList.Count}");
                
                foreach (var collection in collectionList)
                {
                    Console.WriteLine($"   - {collection}");
                }
                
                // Teste de inserção em coleção temporária
                var testCollection = database.GetCollection<dynamic>("connection_test");
                
                var testDoc = new
                {
                    test = true,
                    timestamp = DateTime.UtcNow,
                    message = "Teste de conexão .NET"
                };
                
                await testCollection.InsertOneAsync(testDoc);
                Console.WriteLine("✅ Teste de inserção realizado");
                
                // Contar documentos de teste
                var count = await testCollection.CountDocumentsAsync("{}");
                Console.WriteLine($"📊 Documentos na coleção teste: {count}");
                
                // Limpar teste
                await testCollection.DeleteManyAsync("{}");
                Console.WriteLine("🧹 Limpeza concluída");
                
                Console.WriteLine("🎉 Conexão Atlas funcionando perfeitamente!");
            }
            catch (MongoAuthenticationException ex)
            {
                Console.WriteLine("❌ Erro de autenticação:");
                Console.WriteLine($"   {ex.Message}");
                Console.WriteLine("💡 Verifique usuário/senha no MongoDB Atlas");
            }
            catch (MongoConfigurationException ex)
            {
                Console.WriteLine("❌ Erro de configuração:");
                Console.WriteLine($"   {ex.Message}");
                Console.WriteLine("💡 Verifique a connection string");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro geral: {ex.Message}");
                
                if (ex.Message.Contains("network"))
                {
                    Console.WriteLine("💡 Verifique IP whitelist (0.0.0.0/0)");
                }
            }
        }
    }
}