using System.Threading.Tasks;

namespace ConsoleExample
{
    public interface ICaculator
    {
        int Add(int x, int y);
        Task<int> AddAsync(int x, int y);
    }
}