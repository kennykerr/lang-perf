windows::include_bindings!();

fn main() -> windows::Result<()> {
    let start = std::time::Instant::now();

    for _ in 0..10_000_000 {
        let _ = Windows::System::Power::PowerManager::RemainingChargePercent()?;
    }

    println!("{} ms", start.elapsed().as_millis());
    Ok(())
}
