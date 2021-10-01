# pylint: disable=missing-module-docstring

import timeit

elapsed = (
    timeit.timeit(
        stmt="w.PowerManager.get_remaining_charge_percent()",
        setup="import winrt.windows.system.power as w;",
        number=10_000_000,
    )
    * 1000
)
print(elapsed)
