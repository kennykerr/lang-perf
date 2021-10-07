fn main() {
    windows::build! {
        Windows::System::Power::PowerManager,
        Windows::System::Diagnostics::*,
        Component::Class,
    };
}
