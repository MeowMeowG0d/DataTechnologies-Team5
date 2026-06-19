using DataTechnologies.Data;
using DataTechnologies.Repositories;
using DataTechnologies.Services;

namespace DataTechnologies;

// Composition root: builds the field cipher once and wires it into every
// repository/service that reads or writes personal data, so encryption is
// applied consistently regardless of which menu action created them.
public sealed class AppServices
{
    public DBConnection DbConnection { get; }
    public string ConnectionString { get; }
    public IFieldCipher Cipher { get; }
    public UserService UserService { get; }
    public AddressService AddressService { get; }
    public OrderRepository OrderRepository { get; }
    public PaymentCardRepository PaymentCardRepository { get; }

    private AppServices(
        DBConnection dbConnection,
        string connectionString,
        IFieldCipher cipher,
        UserService userService,
        AddressService addressService,
        OrderRepository orderRepository,
        PaymentCardRepository paymentCardRepository)
    {
        DbConnection = dbConnection;
        ConnectionString = connectionString;
        Cipher = cipher;
        UserService = userService;
        AddressService = addressService;
        OrderRepository = orderRepository;
        PaymentCardRepository = paymentCardRepository;
    }

    public static AppServices Create(string databaseName)
    {
        var cipher = CryptoService.FromEnvironment();
        var dbConnection = new DBConnection(databaseName);
        string connectionString = dbConnection.ConnectionString;

        var userService = new UserService(new UserRepository(connectionString, cipher), new PasswordHasher());
        var addressService = new AddressService(dbConnection, cipher);
        var orderRepository = new OrderRepository(connectionString, cipher);
        var paymentCardRepository = new PaymentCardRepository(connectionString, cipher);

        return new AppServices(
            dbConnection,
            connectionString,
            cipher,
            userService,
            addressService,
            orderRepository,
            paymentCardRepository);
    }
}
