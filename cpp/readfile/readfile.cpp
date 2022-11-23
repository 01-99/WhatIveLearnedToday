#include <string>
#include <fstream>

using namespace std;

int main()
{
    std::ifstream file("test.txt");
    if (file.is_open())
    {
        std::string line;
        while (std::getline(file, line))
        {
            printf("%s", line.c_str());
        }
        file.close();
    }
}