windows::include_bindings!();

fn main() -> windows::Result<()> {
    let start = std::time::Instant::now();

    for _ in 0..10_000_000 {
        let _ = Windows::System::Power::PowerManager::RemainingChargePercent()?;
    }

    println!("Factory calls: {} ms", start.elapsed().as_millis());

    let start = std::time::Instant::now();
    let object = Component::Class::new()?;

    for _ in 0..10_000_000 {
        object.SetInt32Property(123)?;
        let _ = object.Int32Property()?;
    }

    println!("Int32 parameters: {} ms", start.elapsed().as_millis());

    let start = std::time::Instant::now();

    for _ in 0..10_000_000 {
        object.SetObjectProperty(&object)?;
        let _ = object.ObjectProperty()?;
    }

    println!("Object parameters: {} ms", start.elapsed().as_millis());

    let start = std::time::Instant::now();

    for _ in 0..10_000_000 {
        object.SetStringProperty("value")?;
        let _ = object.StringProperty()?;
    }

    println!("String parameters: {} ms", start.elapsed().as_millis());

    let process = Windows::System::Diagnostics::ProcessDiagnosticInfo::GetForCurrentProcess()?;
    println!("Private pages: {}", process.MemoryUsage()?.GetReport()?.PrivatePageCount()?);

    Ok(())
}
