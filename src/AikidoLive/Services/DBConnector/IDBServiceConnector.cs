using AikidoLive.DataModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AikidoLive.Services.DBConnector
{
    public interface IDBServiceConnector
    {
        Task<List<BlogDocument>> GetBlogPosts();
        Task<bool> CreateBlogDocument(BlogDocument blogDocument);
        Task<bool> UpdateBlogDocument(BlogDocument blogDocument);
        Task<List<LibraryDocument>> GetLibraryTitles();
        Task<List<UserList>> GetUsers();
        Task<bool> UpdateUser(UserList userList);
        Task<List<PlaylistsDocument>> GetPlaylists();
        List<string> GetDatabasesList();
        Task<List<string>> GetDatabasesListAsync();
        Task<List<string>> GetContainersListAsync(string databaseName);
        void Dispose();
    }
}