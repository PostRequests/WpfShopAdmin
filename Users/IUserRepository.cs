// ��������� ������� � �������������.
namespace ConsoleShop
{
    public interface IUserRepository
    {
        List<User> load();              // ��������� ���� �������������
        void save(List<User> users);    // ������������ ������ �������������
    }
}
