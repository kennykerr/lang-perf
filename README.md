## Language overhead calling Windows APIs

Rust (using the [Windows](https://github.com/microsoft/windows-rs) crate) and [C++/WinRT](https://github.com/microsoft/cppwinrt) (standard C++) are on par as zero-overhead implementations so that cost is basically the baseline cost of calling the API.  

![compare](https://user-images.githubusercontent.com/9845234/135662175-95e70b19-3bed-4281-8455-b46f11de2496.png)
