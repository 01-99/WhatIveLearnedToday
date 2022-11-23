#include <iostream>
#include <fstream>

using namespace std;

int main()
{
    std::ofstream outfile("test.txt");

    outfile << "my text here!" << std::endl;

    outfile.close();
}