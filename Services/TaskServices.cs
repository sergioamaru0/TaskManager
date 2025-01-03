using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TaskManager.DTO;
using TaskManager.Model;
using MongoDB.Bson;

namespace TaskManager.Services
{
    public class TaskServices
    {
        private readonly IMongoCollection<TaskModel> _taskCollection;
        public TaskServices(IOptions<DatabaseSettings> databaseSettings)
        {   
            var settings = MongoClientSettings.FromConnectionString(databaseSettings.Value.ConnectionString);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            var client = new MongoClient(settings);      
            try{
                
                var database = client.GetDatabase(databaseSettings.Value.DatabaseName);
                _taskCollection = database.GetCollection<TaskModel>(databaseSettings.Value.CollectionName);
            }catch(MongoException ex){
                throw new MongoException("Error al conectar con la base de datos", ex);
            }
            
            
        }
        public async Task<List<TaskModel>> GetTasks()=>
            await _taskCollection.Find(task => true).ToListAsync();    
        
        public async Task<TaskModel> GetTaskId(String taskId)=>
            await _taskCollection.Find<TaskModel>(task => task.TaskId == taskId).FirstOrDefaultAsync();
        public async Task<List<TaskModel>> GetTasksByStatus(bool status)
        {
            return await _taskCollection.Find(task => task.TaskStatus == status).ToListAsync();
        }
        public async Task<TaskModel> CreateTask(TaskModel taskModel)
        {     
            await _taskCollection.InsertOneAsync(taskModel);
            return taskModel;
        }
        public async Task UpdateTask(string taskId, TaskDto taskDto)
        {
    
            var updateDefinition = Builders<TaskModel>.Update
                .Set(t => t.TaskName, taskDto.TaskName)
                .Set(t => t.TaskDescription, taskDto.TaskDescription)
                .Set(t => t.TaskStatus, taskDto.TaskStatus);

            await _taskCollection.UpdateOneAsync(
                t => t.TaskId == taskId,
                updateDefinition);
           
        }
        public async Task DeleteTask(string taskId)
        {
            await _taskCollection.DeleteOneAsync(t => t.TaskId == taskId);
        }

    }
}