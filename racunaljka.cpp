#include <iostream>
#include <vector>
#include <cstdlib>

class row
{
private:
    int* nums;
    int capacity;
public:
    int sum;
    int size;
    int* all_used_nums()
    {
        int* ret = (int *)malloc((size + 1)* sizeof(int));
        int poz = 0;
        for (int i=0;i<size;i++){
            if (*(nums + sizeof(int)*i) != -1)
            {
                *(ret + poz * sizeof(int)) = *(nums + sizeof(int)*i);
                poz++;
            }
        }
        *(ret + poz * sizeof(int)) = -1;
        return ret;
    }
    void add_num(int num)
    {
        if (capacity == size)
        {
            capacity*=2;
            nums = (int*)realloc(nums, capacity*sizeof(int));
        }
        *(nums + sizeof(int)*size) = num;
        size++;
    }
};



int main(void)
{

}