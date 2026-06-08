using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;

public class Neo4jRepository
{
    private readonly IDriver _driver;

    public Neo4jRepository(IDriver driver)
    {
        _driver = driver ?? throw new ArgumentNullException(nameof(driver));
    }
}