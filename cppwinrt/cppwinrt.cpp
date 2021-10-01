#define WINRT_NO_MODULE_LOCK
#include "winrt/Windows.System.Power.h"

int main()
{
    auto start = std::chrono::high_resolution_clock::now();

    for (int i = 0; i < 10'000'000; i++)
    {
        [[maybe_unused]] auto _ = winrt::Windows::System::Power::PowerManager::RemainingChargePercent();
    }

    printf("%lld ms", std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::high_resolution_clock::now() - start).count());
}
