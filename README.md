## Language overhead calling Windows APIs

Rust (using the [Windows](https://github.com/microsoft/windows-rs) crate) and [C++/WinRT](https://github.com/microsoft/cppwinrt) (standard C++) are mostly on par as zero-overhead implementations so that cost is basically the baseline cost of calling the API. Rust is slightly slower in the string test because Rust prefers UTF-8 for string literals so the added cost here is converting from UTF-8 to HSTRING.

## Work in progress

![image](https://user-images.githubusercontent.com/9845234/137188331-7a81a6a3-aa25-48f0-9379-5a2748db08d3.png)

![image](https://user-images.githubusercontent.com/9845234/137188378-f1829761-1a84-407b-9833-58618dca3aad.png)

![image](https://user-images.githubusercontent.com/9845234/137188403-d9043142-1b98-4cd9-91ef-732fb91a2ee3.png)

![image](https://user-images.githubusercontent.com/9845234/137188428-ba78fd54-7bbe-4875-88bd-4a02312cf885.png)
