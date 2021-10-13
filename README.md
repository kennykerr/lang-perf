## Language overhead calling Windows APIs

Rust (using the [Windows](https://github.com/microsoft/windows-rs) crate) and [C++/WinRT](https://github.com/microsoft/cppwinrt) (standard C++) are mostly on par as zero-overhead implementations so that cost is basically the baseline cost of calling the API. Rust is slightly slower in the string test because Rust prefers UTF-8 for string literals so the added cost here is converting from UTF-8 to HSTRING.

## Work in progress

![image](https://user-images.githubusercontent.com/9845234/137162331-4051db08-ba18-4a48-b960-7f83e4a5e7a6.png)

![image](https://user-images.githubusercontent.com/9845234/137162375-00071040-fecc-41cf-be59-2752d218f2ae.png)

![image](https://user-images.githubusercontent.com/9845234/137162612-5ea40cf2-5cad-4a8b-87d5-8bd2408da95c.png)

![image](https://user-images.githubusercontent.com/9845234/137162641-3b104e21-8f47-4f91-9612-1bccf2b8baa6.png)
