namespace FocusMind.DBContext.Repositories;

// Traduce la violación de UQ_TM_PEDIDO_NUMEROPEDIDO (SQL Server error 2627/2601) a una
// excepción de dominio legible en vez de dejar propagar el SqlException crudo hacia Business.
public sealed class NumeroPedidoDuplicadoException() : Exception("El número de pedido ya existe.");
