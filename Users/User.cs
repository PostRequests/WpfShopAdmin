// ������ ������������.
namespace ConsoleShop
{
    public class User
    {
        public string name { get; set; } = "";         // ���
        public string phone { get; set; } = "";        // ������� (8XXXXXXXXXX)
        public string passwordHash { get; set; } = ""; // SHA-256 �� ������
        public bool isAdmin { get; set; }              // ����� ��������������
    }
}
