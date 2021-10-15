using System;

namespace Vektonn.DataSource.Kafka
{
    public class KafkaProducerException : Exception
    {
        public KafkaProducerException(string message)
            : base(message)
        {
        }
    }
}
