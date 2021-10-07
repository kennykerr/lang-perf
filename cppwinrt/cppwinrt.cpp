#define WINRT_NO_MODULE_LOCK
#include "winrt/Windows.System.Power.h"
#include "winrt/Windows.System.Diagnostics.h"
#include "winrt/Component.h"

int main()
{
    auto start = std::chrono::high_resolution_clock::now();

    for (int i = 0; i < 10'000'000; i++)
    {
        [[maybe_unused]] auto _ = winrt::Windows::System::Power::PowerManager::RemainingChargePercent();
    }

    printf("Factory calls: %lld ms\n", std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::high_resolution_clock::now() - start).count());

    start = std::chrono::high_resolution_clock::now();
    auto object = winrt::Component::Class();

    for (int i = 0; i < 10'000'000; i++)
    {
        object.Int32Property(123);
        [[maybe_unused]] auto _ = object.Int32Property();
    }

    printf("Int32 parameters: %lld ms\n", std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::high_resolution_clock::now() - start).count());

    start = std::chrono::high_resolution_clock::now();

    for (int i = 0; i < 10'000'000; i++)
    {
        object.ObjectProperty(object);
        [[maybe_unused]] auto _ = object.ObjectProperty();
    }

    printf("Object parameters: %lld ms\n", std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::high_resolution_clock::now() - start).count());

    auto process = winrt::Windows::System::Diagnostics::ProcessDiagnosticInfo::GetForCurrentProcess();
    printf("Private pages: %lld\n", process.MemoryUsage().GetReport().PrivatePageCount());
}
